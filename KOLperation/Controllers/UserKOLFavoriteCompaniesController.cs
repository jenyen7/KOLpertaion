using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using KOLperation.Models;
using KOLperation.Utils;
using static KOLperation.Utils.Enum;

namespace KOLperation.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UserKOLFavoriteCompaniesController : ApiController
    {
        private readonly AModel db = new AModel();
        private readonly string kol = ConfigurationManager.AppSettings["iamkol"].ToString();

        // GET: api/UserKOLFavoriteCompanies
        [HttpGet]
        [Route("api/GetKOLFavoriteCompanies")]
        public IHttpActionResult GetKOLFavoriteCompanies()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol))
            {
                return BadRequest("No Permission");
            }
            return Ok(db.KOLFavoriteCompanies.Where(k => k.KOLId == currentUser.UserId).OrderByDescending(o => o.Record).Select(s => new
            {
                s.CompanyId,
                s.UserCompany.Guid,
                s.UserCompany.Company,
                s.UserCompany.CompanyLogo,
                s.Record,
                CoopTimes = s.UserKOL.Coops.Where(c => c.SponsoredContent.CompanyId == s.CompanyId && c.Status == (int)Status.雙方確認).Count(),
                Sectors = db.TagSectors.Where(ts => s.UserCompany.SectorTags.Contains(ts.TagId.ToString())).Select(st => new
                {
                    st.TagName,
                    st.TagIcon
                })
            }));
        }

        // PUT: api/UserKOLFavoriteCompanies/5
        [HttpPut]
        [Route("api/AddThisCompanyToMyFavorites/{id}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUserKOLFavoriteCompany(int id)
        {
            UserCompany company = db.UserCompanies.FirstOrDefault(f => f.ComId == id);
            if (company == null)
            {
                return NotFound();
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol))
            {
                return BadRequest("No Permission");
            }
            UserKOLFavoriteCompany check = db.KOLFavoriteCompanies.FirstOrDefault(f => f.KOLId == currentUser.UserId && f.CompanyId == id);
            if (check != null)
            {
                return BadRequest("already added");
            }
            UserKOLFavoriteCompany favoriteCompany = new UserKOLFavoriteCompany
            {
                KOLId = currentUser.UserId,
                CompanyId = id,
                Record = DateTime.Now
            };
            db.KOLFavoriteCompanies.Add(favoriteCompany);
            db.SaveChanges();
            return Ok("added");
        }

        // DELETE: api/UserKOLFavoriteCompanies/5
        [HttpDelete]
        [Route("api/RemoveThisCompanyFromMyFavorites/{id}")]
        [ResponseType(typeof(UserKOLFavoriteCompany))]
        public IHttpActionResult DeleteUserKOLFavoriteCompany(int id)
        {
            UserCompany company = db.UserCompanies.FirstOrDefault(f => f.ComId == id);
            if (company == null)
            {
                return NotFound();
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol))
            {
                return BadRequest("No Permission");
            }
            UserKOLFavoriteCompany userKOLFavoriteCompany = db.KOLFavoriteCompanies.FirstOrDefault(f => f.KOLId == currentUser.UserId && f.CompanyId == id);
            if (userKOLFavoriteCompany == null)
            {
                return NotFound();
            }
            db.KOLFavoriteCompanies.Remove(userKOLFavoriteCompany);
            db.SaveChanges();
            return Ok("removed");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}