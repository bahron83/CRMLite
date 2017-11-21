using System;
using System.Collections.Generic;
using System.Text;
using Westwind.Utilities;
using Westwind.BusinessObjects;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CRMLiteBusiness
{
    public class ContactRepository : EntityFrameworkRepository<CRMLiteContext, Contact>
    {
        public ContactRepository(CRMLiteContext context) : base(context) { }

        public override async Task<Contact> Load(object contactId)
        {
            Contact contact = null;
            try
            {
                int id = (int)contactId;
                contact = await Context.Contacts
                    .Include(ctx => ctx.Company)
                    .Include(ctx => ctx.Activities)
                    .FirstOrDefaultAsync(con => con.ContactID == id);

                if (contact != null)
                    OnAfterLoaded(contact);
            }
            catch (InvalidOperationException)
            {
                // Handles errors where an invalid Id was passed, but SQL is valid                
                SetError("Couldn't load album - invalid album id specified.");
                return null;
            }
            catch (Exception ex)
            {
                // handles Sql errors                                
                SetError(ex);
            }

            return contact;
        }

        public async Task<List<Contact>> GetAllContacts(int page = 0, int pageSize = 15)
        {
            IQueryable<Contact> contacts = Context.Contacts
                .Include(ctx => ctx.Company)
                .Include(ctx => ctx.Activities)
                .OrderBy(con => con.LastName);

            if (page > 0)
            {
                contacts = contacts
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);
            }

            return await contacts.ToListAsync();
        }

        public async Task<Contact> SaveContact(Contact postedContact)
        {
            int id = postedContact.ContactID;

            Contact contact;

            if (id < 1)
                contact = Create();
            else
            {
                contact = await Load(id);
                if (contact == null)
                    contact = Create();
            }

            // check for existing comapny and assign if matched
            if (contact.Company.CompanyID < 1)
            {
                var company = await Context.Companies
                    .FirstOrDefaultAsync(com => com.Name == postedContact.Company.Name);
                if (company != null)
                    contact.Company.CompanyID = company.CompanyID;
            }

            DataUtils.CopyObjectData(postedContact.Company, contact.Company, "Id");

            // new company 
            if (contact.Company.CompanyID < 1)
                Context.Companies.Add(contact.Company);

            contact.CompanyID = contact.Company.CompanyID;
            DataUtils.CopyObjectData(postedContact, contact, "Activities,Company,ContactID,CompanyID");            

            //now lets save it all
            if (!await SaveAsync())
                return null;

            return contact;
        }

        public async Task<bool> DeleteContact(int id)
        {
            using (var tx = Context.Database.BeginTransaction())
            {
                // manually delete calls
                var calls = await Context.Calls.Where(a => a.ContactID == id).ToListAsync();
                for (int i = calls.Count - 1; i > -1; i--)
                {
                    var call = calls[i];
                    calls.Remove(call);
                    Context.Calls.Remove(call);
                }

                // manually delete visits
                var visits = await Context.Visits.Where(a => a.ContactID == id).ToListAsync();
                for (int i = visits.Count - 1; i > -1; i--)
                {
                    var visit = visits[i];
                    visits.Remove(visit);
                    Context.Visits.Remove(visit);
                }

                var contact = await Context.Contacts
                    .FirstOrDefaultAsync(c => c.ContactID == id);

                if (contact == null)
                {
                    SetError("Invalid contact id.");
                    return false;
                }

                Context.Contacts.Remove(contact);

                var result = await SaveAsync();
                if (result)
                    tx.Commit();

                return result;
            }
        }

        protected override bool OnValidate(Contact entity)
        {
            if (entity == null)
            {
                ValidationErrors.Add("No item was passed.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.LastName))
                ValidationErrors.Add("Please enter a name for this contact.", "LastName");
            if (string.IsNullOrEmpty(entity.Email))
                ValidationErrors.Add("Please enter an email address for this contact.", "Email");

            return ValidationErrors.Count < 1;
        }
    }
}
