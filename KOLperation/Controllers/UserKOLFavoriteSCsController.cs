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

namespace KOLperation.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UserKOLFavoriteSCsController : ApiController
    {
        private readonly AModel db = new AModel();
        private readonly string iconUrl = ConfigurationManager.AppSettings["ourIconUrl"].ToString();
        private readonly string kol = ConfigurationManager.AppSettings["iamkol"].ToString();

        // GET: api/UserKOLFavoriteSCs
        [HttpGet]
        [Route("api/GetKOLFavoriteSCs")]
        public IHttpActionResult GetKOLFavoriteSCs()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol))
            {
                return BadRequest("No Permission");
            }
            return Ok(db.KOLFavoriteSCs.Where(k => k.KOLId == currentUser.UserId).OrderByDescending(o => o.Record).Select(s => new
            {
                SponsoredContentId = s.SponsoredContent.ScId,
                s.SponsoredContent.Title,
                s.SponsoredContent.Detail,
                s.SponsoredContent.EndTime,
                Status = s.SponsoredContent.SCstatus,
                s.SponsoredContent.UserCompany.Company,
                ProductPicture = s.SponsoredContent.ProductPicture,
                Channels = db.TagChannels.Where(c => s.SponsoredContent.ChannelTags.Contains(c.TagId.ToString())).Select(ct => new
                {
                    ct.FAid,
                    ct.TagName,
                    Icon = iconUrl + ct.TagIcon
                }),
                Sectors = db.TagSectors.Where(t => s.SponsoredContent.SectorTags.Contains(t.TagId.ToString())).Select(tt => new
                {
                    tt.TagName,
                    tt.TagIcon
                })
            }));
        }

        // PUT: api/UserKOLFavoriteSCs/5
        [HttpPut]
        [Route("api/AddThisSCToMyFavorites/{id}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUserKOLFavoriteSC(int id)
        {
            SponsoredContent sc = db.SponsoredContents.FirstOrDefault(f => f.ScId == id);
            if (sc == null)
            {
                return NotFound();
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol))
            {
                return BadRequest("No Permission");
            }
            UserKOLFavoriteSC check = db.KOLFavoriteSCs.FirstOrDefault(f => f.KOLId == currentUser.UserId && f.SponsoredContentId == id);
            if (check != null)
            {
                return BadRequest("already added");
            }
            UserKOLFavoriteSC favoriteSC = new UserKOLFavoriteSC
            {
                KOLId = currentUser.UserId,
                SponsoredContentId = id,
                Record = DateTime.Now
            };
            db.KOLFavoriteSCs.Add(favoriteSC);
            db.SaveChanges();
            return Ok("added");
        }

        // DELETE: api/UserKOLFavoriteSCs/5
        [HttpDelete]
        [Route("api/RemoveThisSCFromMyFavorites/{id}")]
        [ResponseType(typeof(UserKOLFavoriteSC))]
        public IHttpActionResult DeleteUserKOLFavoriteSC(int id)
        {
            SponsoredContent sc = db.SponsoredContents.FirstOrDefault(f => f.ScId == id);
            if (sc == null)
            {
                return NotFound();
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol))
            {
                return BadRequest("No Permission");
            }
            UserKOLFavoriteSC userKOLFavoriteSC = db.KOLFavoriteSCs.FirstOrDefault(f => f.KOLId == currentUser.UserId && f.SponsoredContentId == id);
            if (userKOLFavoriteSC == null)
            {
                return NotFound();
            }
            db.KOLFavoriteSCs.Remove(userKOLFavoriteSC);

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