using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Westwind.BusinessObjects;
using Westwind.Utilities;

namespace CRMLiteBusiness
{
    public class CompanyRepository : EntityFrameworkRepository<CRMLiteContext,Company>
    {
        public CompanyRepository(CRMLiteContext context) : base(context) { }

        public override async Task<Company> Load(object companyId)
        {
            Company company = null;
            try
            {
                int id = (int)companyId;
                company = await Context.Companies
                    .Include(ctx => ctx.Contacts)
                    .Include(ctx => ctx.Contracts)
                    .FirstOrDefaultAsync(com => com.CompanyID == id);

                if (company != null)
                    OnAfterLoaded(company);
            }
            catch (InvalidOperationException)
            {
                // Handles errors where an invalid Id was passed, but SQL is valid                
                SetError("Couldn't load company - invalid company id specified.");
                return null;
            }
            catch (Exception ex)
            {
                // handles Sql errors                                
                SetError(ex);
            }

            return company;
        }

        public async Task<List<Company>> GetAllCompanies(int page = 0, int pageSize = 25)
        {
            IQueryable<Company> companies = Context.Companies
                .Include(ctx => ctx.Contacts)
                .Include(ctx => ctx.Contracts)
                .OrderBy(com => com.Name);

            if (page > 0)
            {
                companies = companies
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);
            }

            return await companies.ToListAsync();
        }

        public async Task<List<CompanyWithContactCount>> GetAllCompanies()
        {
            return await Context.Companies
                .OrderBy(com => com.Name)
                .Select(com => new CompanyWithContactCount()
                {
                    CompanyID = com.CompanyID,
                    Name = com.Name,
                    VatNumber = com.VatNumber,
                    Address = com.Address,
                    City = com.City,
                    ContactCount = Context.Contacts.Count(con => con.CompanyID == com.CompanyID)
                })
                .ToListAsync();
        }

        public async Task<Company> SaveCompany(Company postedCompany)
        {
            int id = postedCompany.CompanyID;

            Company company;

            if (id < 1)
                company = Create();
            else
            {
                company = await Load(id);
                if (company == null)
                    company = Create();
            }
            

            // add or udpate contacts
            foreach (var postedContact in postedCompany.Contacts)
            {
                var contact = company.Contacts.FirstOrDefault(con => con.ContactID == postedContact.ContactID);
                if (postedContact.ContactID > 0 && contact != null)
                    DataUtils.CopyObjectData(postedContact, contact);
                else
                {
                    contact = new Contact();
                    Context.Contacts.Add(contact);
                    DataUtils.CopyObjectData(postedContact, contact, "Id,Contracts");
                    company.Contacts.Add(contact);
                }
            }

            // then find all deleted contacts not in new contacts
            var deletedContacts = company.Contacts
                .Where(cnt => cnt.ContactID > 0 &&
                                !postedCompany.Contacts
                                    .Where(c => c.ContactID > 0)
                                    .Select(c => c.ContactID)
                                .Contains(cnt.ContactID))
                .ToList();

            foreach (var dcontact in deletedContacts)
                company.Contacts.Remove(dcontact);

            
            

            //now lets save it all
            if (!await SaveAsync())
                return null;

            return company;
        }

        public async Task<bool> DeleteCompany(int id)
        {
            using (var ctx = Context.Database.BeginTransaction())
            {
                // manually delete contacts
                var contacts = await Context.Contacts.Where(t => t.CompanyID == id).ToListAsync();
                for (int i = contacts.Count - 1; i > -1; i--)
                {
                    var contact = contacts[i];
                    contacts.Remove(contact);
                    Context.Contacts.Remove(contact);
                }

                var company = await Context.Companies
                    .FirstOrDefaultAsync(a => a.CompanyID == id);

                if (company == null)
                {
                    SetError("Invalid company id.");
                    return false;
                }

                Context.Companies.Remove(company);

                var result = await SaveAsync();
                if (result)
                    ctx.Commit();

                return result;
            }
        }

        public async Task<List<Contact>> GetContactsForCompany(int companyId)
        {
            return await Context.Contacts
                .Include(c => c.Activities)
                .Include(c => c.Company)
                .Where(c => c.CompanyID == companyId)
                .ToListAsync();
        }

        public async Task<List<CompanyLookupItem>> CompanyLookup(string search = null)
        {
            if (string.IsNullOrEmpty(search))
                return new List<CompanyLookupItem>();

            var repo = new ContactRepository(Context);

            var term = search.ToLower();
            return await repo.Context.Companies
                .Where(c => c.Name.ToLower().StartsWith(term))
                .Select(c => new CompanyLookupItem
                {
                    name = c.Name,
                    id = c.CompanyID
                })
                .ToListAsync();
        }

        protected override bool OnValidate(Company entity)
        {
            if (entity == null)
            {
                ValidationErrors.Add("No item was passed.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.Name))
                ValidationErrors.Add("Please enter a name for this company.", "Name");
            else if (string.IsNullOrEmpty(entity.VatNumber) || entity.VatNumber.Length < 30)
                ValidationErrors.Add("Please provide a VatNumber.");
            //else if (entity.Tracks.Count < 1)
                //ValidationErrors.Add("Album must have at least one song associated.");

            return ValidationErrors.Count < 1;
        }

        public class CompanyLookupItem
        {
            public string name { get; set; }
            public int id { get; set; }
        }
    }
}
