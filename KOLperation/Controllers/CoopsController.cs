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
using KOLperation.Middleware;
using KOLperation.Models;
using KOLperation.Utils;
using static KOLperation.Utils.Enum;

namespace KOLperation.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class CoopsController : ApiController
    {
        private readonly AModel db = new AModel();
        private readonly string kol = ConfigurationManager.AppSettings["iamkol"].ToString();
        private readonly string company = ConfigurationManager.AppSettings["iamcompany"].ToString();

        // GET: api/Coops
        [HttpGet]
        [Route("api/GetKOLsentCoop")]
        public IHttpActionResult GetKOLsentCoop()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol)) return BadRequest("No Permission");
            return Ok(db.Coops.Where(u => u.KOLId == currentUser.UserId && (u.Status == (int)Status.等公司確認 || u.Status == (int)Status.公司拒絕)).Select(c => new
            {
                c.CoopId,
                c.SponsoredContentId,
                MsgId = c.MessageHistoryId,
                CoopStatus = c.Status,
                CaseStatus = c.SponsoredContent.SCstatus,
                CaseTitle = c.SponsoredContent.Title,
                CompanyName = c.SponsoredContent.UserCompany.Company,
                c.SponsoredContent.UserCompany.CompanyLogo,
                ProductPic = c.SponsoredContent.ProductPicture,
                CaseDetail = c.SponsoredContent.Detail,
                Favorite = db.KOLFavoriteSCs.FirstOrDefault(f => f.SponsoredContentId == c.SponsoredContentId && f.KOLId == currentUser.UserId) != null,
            }));
        }

        [HttpGet]
        [Route("api/GetKOLinvitatedCoop")]
        public IHttpActionResult GetKOLinvitatedCoop()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol)) return BadRequest("No Permission");
            return Ok(db.Coops.Where(u => u.KOLId == currentUser.UserId && u.Status == (int)Status.等KOL確認).Select(c => new
            {
                c.CoopId,
                c.SponsoredContentId,
                MsgId = c.MessageHistoryId,
                CaseStatus = c.SponsoredContent.SCstatus,
                CaseTitle = c.SponsoredContent.Title,
                CompanyName = c.SponsoredContent.UserCompany.Company,
                c.SponsoredContent.UserCompany.CompanyLogo,
                ProductPic = c.SponsoredContent.ProductPicture,
                CaseDetail = c.SponsoredContent.Detail,
                Favorite = db.KOLFavoriteSCs.FirstOrDefault(f => f.SponsoredContentId == c.SponsoredContentId && f.KOLId == currentUser.UserId) != null,
            }));
        }

        [HttpGet]
        [Route("api/GetKOLsuccessfulCasesTop10")]
        public IHttpActionResult GetKOLsuccessfulCoopsTop10()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol)) return BadRequest("No Permission");
            return Ok(db.Coops.Where(u => u.KOLId == currentUser.UserId && u.Status == (int)Status.雙方確認).Select(c => new
            {
                c.SponsoredContentId,
                MsgId = c.MessageHistoryId,
                CaseStatus = c.SponsoredContent.SCstatus,
                CaseTitle = c.SponsoredContent.Title,
                CompanyName = c.SponsoredContent.UserCompany.Company,
                c.SponsoredContent.UserCompany.CompanyLogo,
                ProductPic = c.SponsoredContent.ProductPicture,
                CaseDetail = c.SponsoredContent.Detail,
                Favorite = db.KOLFavoriteSCs.FirstOrDefault(f => f.SponsoredContentId == c.SponsoredContentId && f.KOLId == currentUser.UserId) != null,
                SuccessDate = c.CoopSuccessDate
            }).OrderByDescending(o => o.SuccessDate).Take(10));
        }

        [HttpGet]
        [Route("api/GetKOLsuccessfulCases")]
        public IHttpActionResult GetKOLsuccessfulCoops()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol)) return BadRequest("No Permission");
            return Ok(db.Coops.Where(u => u.KOLId == currentUser.UserId && u.Status == (int)Status.雙方確認).Select(c => new
            {
                c.SponsoredContentId,
                MsgId = c.MessageHistoryId,
                CaseStatus = c.SponsoredContent.SCstatus,
                CaseTitle = c.SponsoredContent.Title,
                CompanyName = c.SponsoredContent.UserCompany.Company,
                c.SponsoredContent.UserCompany.CompanyLogo,
                ProductPic = c.SponsoredContent.ProductPicture,
                CaseDetail = c.SponsoredContent.Detail,
                Favorite = db.KOLFavoriteSCs.FirstOrDefault(f => f.SponsoredContentId == c.SponsoredContentId && f.KOLId == currentUser.UserId) != null,
                SuccessDate = c.CoopSuccessDate
            }).OrderByDescending(o => o.SuccessDate).Skip(10));
        }

        //公司首頁上方
        [HttpGet]
        [Route("api/GetCompanyOnGoingCoopsDetailView")]
        public IHttpActionResult GetCompanyOnGoingCoopsDetailView()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(company)) return BadRequest("No Permission");
            return Ok(db.SponsoredContents.Where(sc => sc.CompanyId == currentUser.UserId).Select(c => new
            {
                SponsoredContentId = c.ScId,
                CaseTitle = c.Title,
                PeopleRequired = c.PeopleRequired,
                KOLsAppliedNum = c.Coops.Where(x => x.Status == (int)Status.等公司確認).Count(),
                KOLsCoopConfirmationNum = c.Coops.Where(x => x.Status == (int)Status.雙方確認).Count(),
                ProductPic = c.ProductPicture
            }).GroupBy(g => g.SponsoredContentId).Select(grp => grp.FirstOrDefault()));
        }

        [HttpGet]
        [Route("api/GetCompanySuccessfulCasesTop10")]
        public IHttpActionResult GetCompanySuccessfulCoopsTop10()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(company)) return BadRequest("No Permission");
            return Ok(db.Coops.Where(u => u.SponsoredContent.CompanyId == currentUser.UserId).Select(c => new
            {
                c.SponsoredContentId,
                MsgId = c.MessageHistoryId,
                CaseStatus = c.SponsoredContent.SCstatus,
                CaseTitle = c.SponsoredContent.Title,
                ProductPic = c.SponsoredContent.ProductPicture,
                CaseDetail = c.SponsoredContent.Detail,
                SuccessDate = c.CoopSuccessDate,
                PeopleCoopWithNum = c.SponsoredContent.Coops.Where(s => s.Status == (int)Status.雙方確認).Count(),
                Sectors = db.TagSectors.Where(ss => c.SponsoredContent.SectorTags.Contains(ss.TagId.ToString())).Select(st => new
                {
                    st.TagName,
                    st.TagIcon
                })
            }).Where(w => w.CaseStatus == 0).GroupBy(g => g.SponsoredContentId).Select(grp => grp.FirstOrDefault()).OrderByDescending(o => o.SuccessDate).Take(10));
        }

        [HttpGet]
        [Route("api/GetCompanySuccessfulCases")]
        public IHttpActionResult GetCompanySuccessfulCoops()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(company)) return BadRequest("No Permission");
            return Ok(db.Coops.Where(u => u.SponsoredContent.CompanyId == currentUser.UserId).Select(c => new
            {
                c.SponsoredContentId,
                MsgId = c.MessageHistoryId,
                CaseStatus = c.SponsoredContent.SCstatus,
                CaseTitle = c.SponsoredContent.Title,
                ProductPic = c.SponsoredContent.ProductPicture,
                CaseDetail = c.SponsoredContent.Detail,
                SuccessDate = c.CoopSuccessDate,
                PeopleCoopWithNum = c.SponsoredContent.Coops.Where(s => s.Status == (int)Status.雙方確認).Count(),
                Sectors = db.TagSectors.Where(ss => c.SponsoredContent.SectorTags.Contains(ss.TagId.ToString())).Select(st => new
                {
                    st.TagName,
                    st.TagIcon
                })
            }).Where(w => w.CaseStatus == 0).GroupBy(g => g.SponsoredContentId).Select(grp => grp.FirstOrDefault()).OrderByDescending(o => o.SuccessDate).Skip(10));
        }

        [HttpPut]
        [Route("api/KolAppliedTo/{id}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCoopByKOL(int id)
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol)) return BadRequest("No Permission");
            SponsoredContent sponsoredContent = db.SponsoredContents.FirstOrDefault(f => f.ScId == id);
            if (sponsoredContent == null) { return BadRequest("沒這個說"); }
            Coop checkCoopPresence = db.Coops.FirstOrDefault(c => c.SponsoredContentId == id && c.KOLId == currentUser.UserId);
            MessageHistory checkMessagePresence = db.MessageHistories.FirstOrDefault(m => m.SponsoredContentId == id && m.KolId == currentUser.UserId);
            int messageId = 0;
            if (checkMessagePresence != null) { messageId = checkMessagePresence.MsgId; }
            if (checkCoopPresence == null)
            {
                Coop newCoop = new Coop
                {
                    Status = (int)Status.等公司確認,
                    MessageHistoryId = messageId,
                    KOLId = currentUser.UserId,
                    SponsoredContentId = id,
                };
                db.Coops.Add(newCoop);
                db.SaveChanges();
                return Ok("new");
            }
            else if (checkCoopPresence.Status == (int)Status.等KOL確認)
            {
                checkCoopPresence.MessageHistoryId = messageId;
                checkCoopPresence.Status = (int)Status.雙方確認;
                checkCoopPresence.CoopSuccessDate = DateTime.Now;
                db.SaveChanges();
                return Ok("matched");
            }
            else
            {
                return Ok("status unchanged,已被拒絕or已報名還在等or已媒合了");
            }
        }

        [HttpPut]
        [Route("api/CompanyInvited/{id}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCoopByCompany(int id, Coop coop)
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(company)) return BadRequest("No Permission");
            SponsoredContent sponsoredContent = db.SponsoredContents.FirstOrDefault(f => f.ScId == coop.SponsoredContentId);
            if (sponsoredContent == null) return BadRequest("沒這個說");
            if (sponsoredContent.CompanyId != currentUser.UserId) return BadRequest("No permission");
            Coop checkCoopPresence = db.Coops.FirstOrDefault(c => c.SponsoredContentId == coop.SponsoredContentId && c.KOLId == id);
            MessageHistory checkMessagePresence = db.MessageHistories.FirstOrDefault(m => m.SponsoredContentId == coop.SponsoredContentId && m.KolId == id);
            int messageId = 0;
            if (checkMessagePresence != null) { messageId = checkMessagePresence.MsgId; }
            if (checkCoopPresence == null)
            {
                Coop newCoop = new Coop
                {
                    Status = (int)Status.等KOL確認,
                    MessageHistoryId = messageId,
                    KOLId = id,
                    SponsoredContentId = coop.SponsoredContentId
                };
                db.Coops.Add(newCoop);
                db.SaveChanges();
                return Ok("new");
            }
            else if (checkCoopPresence.Status == (int)Status.等公司確認)
            {
                if (currentUser.UserId != checkCoopPresence.SponsoredContent.CompanyId)
                {
                    return BadRequest("access denied");
                }
                checkCoopPresence.MessageHistoryId = messageId;
                checkCoopPresence.Status = (int)Status.雙方確認;
                checkCoopPresence.CoopSuccessDate = DateTime.Now;
                db.SaveChanges();
                return Ok("matched");
            }
            else
            {
                return Ok("status unchanged,已拒絕別人or已邀請還在等or已媒合了");
            }
        }

        [HttpDelete]
        [Route("api/KolRefused/{id}")]
        public IHttpActionResult DeleteCoopByKOL(int id)
        {
            Coop coop = db.Coops.Find(id);
            if (coop == null)
            {
                return NotFound();
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (currentUser.UserId != coop.KOLId)
            {
                return BadRequest("No Permission");
            }
            if (coop.Status == (int)Status.雙方確認)
            {
                return Ok("cannot change status");
            }
            db.Coops.Remove(coop);
            db.SaveChanges();
            return Ok("removed");
        }

        [HttpDelete]
        [Route("api/CompanyRefused/{id}")]
        public IHttpActionResult DeleteCoopByCompany(int id)
        {
            Coop coop = db.Coops.Find(id);
            if (coop == null)
            {
                return NotFound();
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (currentUser.UserId != coop.SponsoredContent.CompanyId)
            {
                return BadRequest("No Permission");
            }
            if (coop.Status == (int)Status.雙方確認)
            {
                return Ok("cannot change status");
            }
            coop.Status = (int)Status.公司拒絕;
            db.SaveChanges();
            return Ok("status changed");
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