using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRMLiteBusiness;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.Text;

namespace CRMLiteNetCore
{
    [ServiceFilter(typeof(ApiExceptionFilter))]
    [EnableCors("CorsPolicy")]
    public class CRMLiteApiController : Controller
    {
        CRMLiteContext context;
        IServiceProvider serviceProvider;

        CompanyRepository CompanyRepo;
        
        IConfiguration Configuration;
        private ILogger<CRMLiteApiController> Logger;

        private IHostingEnvironment HostingEnv;

        public CRMLiteApiController(
            CRMLiteContext ctx,
            IServiceProvider svcProvider,
            CompanyRepository companyRepo,
            
            IConfiguration config,
            ILogger<CRMLiteApiController> logger,
            IHostingEnvironment env)
        {
            context = ctx;
            serviceProvider = svcProvider;
            Configuration = config;

            CompanyRepo = companyRepo;
            
            Logger = logger;

            HostingEnv = env;
        }

        [HttpGet]
        [Route("api/throw")]
        public object Throw()
        {
            throw new InvalidOperationException("This is an unhandled exception");
        }

        #region companies        

        [HttpGet]
        [Route("api/companies")]
        public async Task<IEnumerable<Company>> GetCompanies(int page = -1, int pageSize = 15)
        {
            //var repo = new AlbumRepository(context);
            return await CompanyRepo.GetAllCompanies(page, pageSize);
        }

        [HttpGet("api/company/{id:int}")]
        public async Task<Company> GetCompany(int id)
        {
            return await CompanyRepo.Load(id);
        }

        [HttpPost("api/album")]
        public async Task<Company> SaveAlbum([FromBody] Company postedAlbum)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            if (!ModelState.IsValid)
                throw new ApiException("Model binding failed.", 500);

            if (!CompanyRepo.Validate(postedAlbum))
                throw new ApiException(CompanyRepo.ErrorMessage, 500, CompanyRepo.ValidationErrors);

            // this doesn't work for updating the child entities properly
            //if(!await AlbumRepo.SaveAsync(postedAlbum))
            //    throw new ApiException(AlbumRepo.ErrorMessage, 500);

            var album = await CompanyRepo.SaveAlbum(postedAlbum);
            if (album == null)
                throw new ApiException(CompanyRepo.ErrorMessage, 500);

            return album;
        }

        [HttpDelete("api/company/{id:int}")]
        public async Task<bool> DeleteCompany(int id)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            return await CompanyRepo.DeleteCompany(id);
        }


        [HttpGet]
        public async Task<string> DeleteAlbumByName(string name)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            var pks =
                await context.Companies.Where(alb => alb.Name == name).Select(com => com.CompanyID).ToAsyncEnumerable().ToList();

            StringBuilder sb = new StringBuilder();
            foreach (int pk in pks)
            {
                bool result = await CompanyRepo.DeleteCompany(pk);
                if (!result)
                    sb.AppendLine(CompanyRepo.ErrorMessage);
            }

            return sb.ToString();
        }

        #endregion

        #region contacts

        [HttpGet]
        [Route("api/contacts")]
        public async Task<IEnumerable> GetContacts()
        {
            return await ArtistRepo.GetAllArtists();
        }

        [HttpGet("api/artist/{id:int}")]
        public async Task<object> Artist(int id)
        {
            var artist = await ArtistRepo.Load(id);

            if (artist == null)
                throw new ApiException("Invalid artist id.", 404);

            var albums = await ArtistRepo.GetAlbumsForArtist(id);

            return new ArtistResponse()
            {
                Artist = artist,
                Albums = albums
            };
        }

        [HttpPost("api/artist")]
        public async Task<ArtistResponse> SaveArtist([FromBody] Artist artist)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            if (!ArtistRepo.Validate(artist))
                throw new ApiException(ArtistRepo.ValidationErrors.ToString(), 500, ArtistRepo.ValidationErrors);

            if (!await ArtistRepo.SaveAsync(artist))
                throw new ApiException("Unable to save artist.");

            return new ArtistResponse()
            {
                Artist = artist,
                Albums = await ArtistRepo.GetAlbumsForArtist(artist.Id)
            };
        }

        [HttpGet("api/artistlookup")]
        public async Task<IEnumerable<object>> ArtistLookup(string search = null)
        {
            if (string.IsNullOrEmpty(search))
                return new List<object>();

            var repo = new ArtistRepository(context);
            var term = search.ToLower();
            return await repo.ArtistLookup(term);
        }


        [HttpDelete("api/artist/{id:int}")]
        public async Task<bool> DeleteArtist(int id)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            return await ArtistRepo.DeleteArtist(id);
        }

        #endregion
    }
}
