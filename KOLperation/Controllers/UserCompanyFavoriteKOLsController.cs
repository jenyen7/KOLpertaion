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
    public class UserCompanyFavoriteKOLsController : ApiController
    {
        private readonly AModel db = new AModel();
        private readonly string iconUrl = ConfigurationManager.AppSettings["ourIconUrl"].ToString();
        private readonly string company = ConfigurationManager.AppSettings["iamcompany"].ToString();

        // GET: api/UserCompanyFavoriteKOLs
        [HttpGet]
        [Route("api/GetCompanyFavoriteKOLs")]
        public IHttpActionResult GetCompanyFavoriteKOLs()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(company))
            {
                return BadRequest("No Permission");
            }
            return Ok(db.CompanyFavoriteKOLs.Where(k => k.CompanyId == currentUser.UserId).OrderByDescending(o => o.Record).Select(s => new
            {
                s.KOLId,
                s.UserKOL.Guid,
                s.UserKOL.Username,
                KOLavatar = s.UserKOL.Avatar,
                CoopTimes = s.UserKOL.Coops.Where(co => co.SponsoredContent.CompanyId == currentUser.UserId && co.Status == (int)Status.雙方確認).Count(),
                s.Record,
                s.UserKOL.Enabled,
                Channels = s.UserKOL.ChannelDetails.Select(ch => new
                {
                    ch.Url,
                    ch.TagChannel.TagName,
                    Icon = iconUrl + ch.TagChannel.TagIcon,
                })
            }));
        }

        // PUT: api/UserCompanyFavoriteKOLs/5
        [HttpPut]
        [Route("api/AddThisKOLToMyFavorites/{id}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUserCompanyFavoriteKOL(int id)
        {
            UserKOL kol = db.UserKOLs.FirstOrDefault(f => f.KolId == id);
            if (kol == null)
            {
                return NotFound();
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(company))
            {
                return BadRequest("No Permission");
            }
            UserCompanyFavoriteKOL check = db.CompanyFavoriteKOLs.FirstOrDefault(f => f.KOLId == id && f.CompanyId == currentUser.UserId);
            if (check != null)
            {
                return BadRequest("already added");
            }
            UserCompanyFavoriteKOL favoriteKOL = new UserCompanyFavoriteKOL
            {
                CompanyId = currentUser.UserId,
                KOLId = id,
                Record = DateTime.Now
            };
            db.CompanyFavoriteKOLs.Add(favoriteKOL);
            db.SaveChanges();
            return Ok("added");
        }

        // DELETE: api/UserCompanyFavoriteKOLs/5
        [HttpDelete]
        [Route("api/RemoveThisKOLFromMyFavorites/{id}")]
        [ResponseType(typeof(UserCompanyFavoriteKOL))]
        public IHttpActionResult DeleteUserCompanyFavoriteKOL(int id)
        {
            UserKOL kol = db.UserKOLs.FirstOrDefault(f => f.KolId == id);
            if (kol == null)
            {
                return NotFound();
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(company))
            {
                return BadRequest("No Permission");
            }
            UserCompanyFavoriteKOL userCompanyFavoriteKOL = db.CompanyFavoriteKOLs.FirstOrDefault(f => f.KOLId == id && f.CompanyId == currentUser.UserId);
            if (userCompanyFavoriteKOL == null)
            {
                return NotFound();
            }
            db.CompanyFavoriteKOLs.Remove(userCompanyFavoriteKOL);
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