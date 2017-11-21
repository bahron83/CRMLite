using CRMLiteBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CRMLiteNetCore
{
    [ServiceFilter(typeof(ApiExceptionFilter))]
    [EnableCors("CorsPolicy")]
    public class CRMLiteApiController : Controller
    {
        CRMLiteContext context;
        IServiceProvider serviceProvider;
        
        CompanyRepository CompanyRepo;
        ContactRepository ContactRepo;
        ContractRepository ContractRepo;
        IConfiguration Configuration;
        private ILogger<CRMLiteApiController> Logger;

        private IHostingEnvironment HostingEnv;

        public CRMLiteApiController(
            CRMLiteContext ctx, 
            IServiceProvider svcProvider,
            CompanyRepository companyRepo, 
            ContactRepository contactRepo,
            ContractRepository contractRepo,
        IConfiguration config,
            ILogger<CRMLiteApiController> logger,
            IHostingEnvironment env)
        {
            context = ctx;
            serviceProvider = svcProvider;
            Configuration = config;

            CompanyRepo = companyRepo;
            ContactRepo = contactRepo;
            ContractRepo = contractRepo;
            Logger = logger;

            HostingEnv = env;
        }

      


        [HttpGet]
        [Route("api/throw")]
        public object Throw()
        {
            throw new InvalidOperationException("This is an unhandled exception");            
        }

        
        #region contacts

        [HttpGet]
        [Route("api/contacts")]
        public async Task<IEnumerable<Contact>> GetContacts(int page = -1, int pageSize = 15)
        {
            //var repo = new AlbumRepository(context);
            return await ContactRepo.GetAllContacts(page, pageSize);
        }

        [HttpGet("api/contact/{id:int}")]
        public async Task<Contact> GetContact(int id)
        {
            return await ContactRepo.Load(id);
        }

        [HttpPost("api/contact")]
        public async Task<Contact> SaveContact([FromBody] Contact postedContact)
        {
            //throw new ApiException("Lemmy says: NO!");

            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            if (!ModelState.IsValid)
                throw new ApiException("Model binding failed.", 500);

            if (!ContactRepo.Validate(postedContact))
                throw new ApiException(ContactRepo.ErrorMessage, 500, ContactRepo.ValidationErrors);

            // this doesn't work for updating the child entities properly
            //if(!await AlbumRepo.SaveAsync(postedAlbum))
            //    throw new ApiException(AlbumRepo.ErrorMessage, 500);

            var contact = await ContactRepo.SaveContact(postedContact);
            if (contact == null)
                throw new ApiException(ContactRepo.ErrorMessage, 500);

            return contact;
        }

        [HttpDelete("api/contact/{id:int}")]
        public async Task<bool> DeleteContact(int id)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            return await ContactRepo.DeleteContact(id);
        }


        [HttpGet]
        public async Task<string> DeleteContactByName(string name)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            var pks =
                await context.Contacts.Where(con => con.LastName == name).Select(con => con.ContactID).ToAsyncEnumerable().ToList();

            StringBuilder sb = new StringBuilder();
            foreach (int pk in pks)
            {
                bool result = await ContactRepo.DeleteContact(pk);
                if (!result)
                    sb.AppendLine(ContactRepo.ErrorMessage);
            }

            return sb.ToString();
        }

        #endregion

        #region contracts

        #endregion


        #region companies

        [HttpGet]
        [Route("api/companies")]
        public async Task<IEnumerable> GetCompanies()
        {
            return await CompanyRepo.GetAllCompanies();
        }

        [HttpGet("api/company/{id:int}")]
        public async Task<object> Company(int id)
        {
            var company = await CompanyRepo.Load(id);

            if (company == null)
                throw new ApiException("Invalid artist id.", 404);

            var contacts = await CompanyRepo.GetContactsForCompany(id);

            return new CompanyResponse()
            {
                Company = company,
                Contacts = contacts
            };
        }

        [HttpPost("api/contact")]
        public async Task<CompanyResponse> SaveArtist([FromBody] Company company)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            if (!CompanyRepo.Validate(company))
                throw new ApiException(CompanyRepo.ValidationErrors.ToString(), 500, CompanyRepo.ValidationErrors);

            if (!await CompanyRepo.SaveAsync(company))
                throw new ApiException("Unable to save company.");

            return new CompanyResponse()
            {
                Company = company,
                Contacts = await CompanyRepo.GetContactsForCompany(company.CompanyID)
            };
        }

        [HttpGet("api/companylookup")]
        public async Task<IEnumerable<object>> CompanyLookup(string search = null)
        {
            if (string.IsNullOrEmpty(search))
                return new List<object>();

            var repo = new CompanyRepository(context);
            var term = search.ToLower();
            return await repo.CompanyLookup(term);
        }


        [HttpDelete("api/company/{id:int}")]
        public async Task<bool> DeleteCompany(int id)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            return await CompanyRepo.DeleteCompany(id);
        }

        #endregion

        #region admin
        [HttpGet]
        [Route("api/reloaddata")]
        public bool ReloadData()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            string isSqLite = Configuration["data:useSqLite"];
            try
            {
                if (isSqLite != "true")
                {
                    context.Database.ExecuteSqlCommand(@"
drop table Tracks;
drop table Albums;
drop table Artists;
drop table Users;
");
                }
                else
                {
                    // this is not reliable for mutliple connections
                    context.Database.CloseConnection();

                    try
                    {
                        System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "CRMLiteData.sqlite"));
                    }
                    catch
                    {
                        throw new ApiException("Can't reset data. Existing database is busy.");
                    }
                }

            }
            catch { }


            CRMLiteDataImporter.EnsureAlbumData(context,
                Path.Combine(HostingEnv.ContentRootPath, 
                "albums.js"));

            return true;
        }
  

        #endregion
    }

    #region Custom Responses

    public class CompanyResponse
    {
        public Company Company { get; set; }

        public List<Contact> Contacts { get; set; }
    }

    #endregion
}

