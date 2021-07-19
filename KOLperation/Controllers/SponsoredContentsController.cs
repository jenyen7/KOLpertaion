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
    public class SponsoredContentsController : ApiController
    {
        private readonly AModel db = new AModel();
        private readonly string iconUrl = ConfigurationManager.AppSettings["ourIconUrl"].ToString();
        private readonly string kol = ConfigurationManager.AppSettings["iamkol"].ToString();
        private readonly string company = ConfigurationManager.AppSettings["iamcompany"].ToString();

        // GET: api/SponsoredContents
        [HttpGet]
        [AcceptVerbs("GET", "POST")]
        [AllowAnonymous]
        [Route("api/GetSponsoredContentsPreview")]
        public IHttpActionResult GetSponsoredContentsTop5([FromUri] SearchTags searchTags)
        {
            var sponsoredContents = db.SponsoredContents.Where(w => w.SCstatus == 1);
            var filtered = FilterByTags(sponsoredContents, searchTags);
            return Ok(filtered.Select(s => new
            {
                SponsoredContentId = s.ScId,
                s.Title,
                s.Budget,
                s.PeopleRequired,
                s.MinimumRequirement,
                s.Detail,
                s.PersonInCharge,
                s.EndTime,
                s.UserCompany.Company,
                s.UserCompany.CompanyLogo,
                Picture = s.ProductPicture,
                Channels = db.TagChannels.Where(cc => s.ChannelTags.Contains(cc.TagId.ToString())).Select(ct => new
                {
                    ct.FAid,
                    ct.TagName,
                    Icon = iconUrl + ct.TagIcon
                }),
                Sectors = db.TagSectors.Where(ss => s.SectorTags.Contains(ss.TagId.ToString())).Select(st => new
                {
                    st.TagName,
                    st.TagIcon
                })
            }).Take(5));
        }

        //KOL視角
        [HttpGet]
        [Route("api/GetSponsoredContentsList")]
        public IHttpActionResult GetSponsoredContents([FromUri] SearchTags searchTags)
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol)) return BadRequest("No Permission");
            var sponsoredContents = db.SponsoredContents.Where(w => w.SCstatus == 1);
            var filtered = FilterByTags(sponsoredContents, searchTags);
            return Ok(filtered.Select(s => new
            {
                SponsoredContentId = s.ScId,
                s.Title,
                s.Budget,
                s.PeopleRequired,
                s.MinimumRequirement,
                s.Detail,
                s.PersonInCharge,
                s.EndTime,
                s.UserCompany.Company,
                s.UserCompany.CompanyLogo,
                ProductPicture = s.ProductPicture,
                Favorite = db.KOLFavoriteSCs.FirstOrDefault(f => f.SponsoredContentId == s.ScId && f.KOLId == currentUser.UserId) != null,
                Channels = db.TagChannels.Where(cc => s.ChannelTags.Contains(cc.TagId.ToString())).Select(ct => new
                {
                    ct.FAid,
                    ct.TagName,
                    Icon = iconUrl + ct.TagIcon
                }),
                Sectors = db.TagSectors.Where(ss => s.SectorTags.Contains(ss.TagId.ToString())).Select(st => new
                {
                    st.TagName,
                    st.TagIcon
                })
            }));
        }

        [HttpGet]
        [Route("api/GetSponsoredContent/{id}")]
        [ResponseType(typeof(SponsoredContent))]
        public IHttpActionResult GetSponsoredContentForKOL(int id)
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol)) return BadRequest("No Permission");
            SponsoredContent sponsoredContent = db.SponsoredContents.FirstOrDefault(f => f.ScId == id);
            if (sponsoredContent == null)
            {
                return NotFound();
            }
            return Ok(new
            {
                SponsoredContentId = sponsoredContent.ScId,
                sponsoredContent.CompanyId,
                sponsoredContent.UserCompany.Guid,
                sponsoredContent.UserCompany.Company,
                sponsoredContent.UserCompany.CompanyLogo,
                sponsoredContent.Title,
                sponsoredContent.Budget,
                sponsoredContent.PeopleRequired,
                sponsoredContent.MinimumRequirement,
                sponsoredContent.Detail,
                sponsoredContent.PersonInCharge,
                sponsoredContent.EndTime,
                ProductPicture = sponsoredContent.ProductPicture,
                Status = sponsoredContent.SCstatus,
                CoopStatus = db.Coops.Where(ww => ww.SponsoredContentId == sponsoredContent.ScId && ww.KOLId == currentUser.UserId).FirstOrDefault() == null ? 42 : db.Coops.Where(ww => ww.SponsoredContentId == sponsoredContent.ScId && ww.KOLId == currentUser.UserId).FirstOrDefault().Status,
                Favorite = db.KOLFavoriteSCs.FirstOrDefault(f => f.SponsoredContentId == sponsoredContent.ScId && f.KOLId == currentUser.UserId) != null,
                Channels = db.TagChannels.Where(cc => sponsoredContent.ChannelTags.Contains(cc.TagId.ToString())).Select(ct => new
                {
                    ct.FAid,
                    ct.TagName,
                    Icon = iconUrl + ct.TagIcon,
                    booling = true
                }),
                Sectors = db.TagSectors.Where(ss => sponsoredContent.SectorTags.Contains(ss.TagId.ToString())).Select(st => new
                {
                    st.TagName,
                    st.TagIcon
                })
            });
        }

        //公司視角
        [HttpGet]
        [Route("api/GetSponsoredContentsByCompany")]
        public IHttpActionResult GetSponsoredContentsByCompany()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(company)) return BadRequest("No Permission");
            return Ok(db.SponsoredContents.Where(w => w.CompanyId == currentUser.UserId && w.SCstatus == 1).Select(s => new
            {
                SponsoredContentId = s.ScId,
                s.Title,
                s.Budget,
                s.PeopleRequired,
                s.MinimumRequirement,
                s.Detail,
                s.PersonInCharge,
                s.EndTime,
                s.SCstatus,
                Picture = s.ProductPicture,
                Channels = db.TagChannels.Where(cc => s.ChannelTags.Contains(cc.TagId.ToString())).Select(ct => new
                {
                    ct.FAid,
                    ct.TagName,
                    Icon = iconUrl + ct.TagIcon
                }),
                Sectors = db.TagSectors.Where(ss => s.SectorTags.Contains(ss.TagId.ToString())).Select(st => new
                {
                    st.TagName,
                    st.TagIcon
                })
            }).OrderByDescending(o => o.SponsoredContentId));
        }

        [HttpGet]
        [Route("api/GetSuccessCoopSC/{id}")]
        [ResponseType(typeof(SponsoredContent))]
        public IHttpActionResult GetClosedCoop(int id)
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            SponsoredContent sponsoredContent = db.SponsoredContents.FirstOrDefault(f => f.ScId == id);
            if (sponsoredContent == null)
            {
                return NotFound();
            }
            if (!currentUser.Role.Equals(company) || currentUser.UserId != sponsoredContent.CompanyId)
            {
                return BadRequest("No Permission");
            }
            return Ok(new
            {
                SponsoredContentId = sponsoredContent.ScId,
                sponsoredContent.Title,
                sponsoredContent.Budget,
                sponsoredContent.PeopleRequired,
                sponsoredContent.MinimumRequirement,
                sponsoredContent.Detail,
                sponsoredContent.PersonInCharge,
                sponsoredContent.EndTime,
                sponsoredContent.UserCompany.Company,
                sponsoredContent.UserCompany.CompanyLogo,
                Picture = sponsoredContent.ProductPicture,
                Channels = db.TagChannels.Where(cc => sponsoredContent.ChannelTags.Contains(cc.TagId.ToString())).Select(ct => new
                {
                    ct.FAid,
                    ct.TagName,
                    Icon = iconUrl + ct.TagIcon
                }),
                Sectors = db.TagSectors.Where(ss => sponsoredContent.SectorTags.Contains(ss.TagId.ToString())).Select(st => new
                {
                    st.TagName,
                    st.TagIcon
                }),
                PeopleCoopWith = sponsoredContent.Coops.Where(w => w.Status == (int)Status.雙方確認).Select(s => new
                {
                    s.UserKOL.Guid,
                    s.UserKOL.Username,
                    KOLavatar = s.UserKOL.Avatar,
                })
            });
        }

        [HttpGet]
        [Route("api/GetOnGoingCoopSC/{id}")]
        [ResponseType(typeof(SponsoredContent))]
        public IHttpActionResult GetOnGoingCoop(int id)
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            SponsoredContent sponsoredContent = db.SponsoredContents.FirstOrDefault(f => f.ScId == id);
            if (sponsoredContent == null)
            {
                return NotFound();
            }
            if (!currentUser.Role.Equals(company) || currentUser.UserId != sponsoredContent.CompanyId)
            {
                return BadRequest("No Permission");
            }
            return Ok(new
            {
                SponsoredContentId = sponsoredContent.ScId,
                sponsoredContent.Title,
                sponsoredContent.Budget,
                sponsoredContent.PeopleRequired,
                sponsoredContent.MinimumRequirement,
                sponsoredContent.Detail,
                sponsoredContent.PersonInCharge,
                sponsoredContent.EndTime,
                sponsoredContent.UserCompany.Company,
                sponsoredContent.UserCompany.CompanyLogo,
                Picture = sponsoredContent.ProductPicture,
                Channels = db.TagChannels.Where(cc => sponsoredContent.ChannelTags.Contains(cc.TagId.ToString())).Select(ct => new
                {
                    ct.FAid,
                    ct.TagName,
                    Icon = iconUrl + ct.TagIcon
                }),
                Sectors = db.TagSectors.Where(ss => sponsoredContent.SectorTags.Contains(ss.TagId.ToString())).Select(st => new
                {
                    st.TagName,
                    st.TagIcon
                }),
                PeopleInvited = sponsoredContent.Coops.Where(w => w.Status == (int)Status.等KOL確認).Select(s => new
                {
                    s.UserKOL.Guid,
                    s.UserKOL.Username,
                    KOLavatar = s.UserKOL.Avatar,
                }),
                PeopleApplied = sponsoredContent.Coops.Where(w => w.Status == (int)Status.等公司確認).Select(s => new
                {
                    s.CoopId,
                    s.KOLId,
                    s.UserKOL.Guid,
                    s.UserKOL.Username,
                    KOLavatar = s.UserKOL.Avatar,
                }),
                PeopleCoopWith = sponsoredContent.Coops.Where(w => w.Status == (int)Status.雙方確認).Select(s => new
                {
                    s.UserKOL.Guid,
                    s.UserKOL.Username,
                    KOLavatar = s.UserKOL.Avatar,
                })
            });
        }

        [HttpPut]
        [Route("api/PutSponsoredContent/{id}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutSponsoredContent(int id, SponsoredContent sponsoredContent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            SponsoredContent data = db.SponsoredContents.FirstOrDefault(f => f.ScId == id);
            if (data == null)
            {
                return NotFound();
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (currentUser.UserId != data.CompanyId)
            {
                return BadRequest("No Permission");
            }
            if (id != sponsoredContent.ScId)
            {
                return BadRequest("Refused");
            }
            data.Title = sponsoredContent.Title ?? data.Title;
            data.Budget = sponsoredContent.Budget ?? data.Budget;
            data.PeopleRequired = sponsoredContent.PeopleRequired ?? data.PeopleRequired;
            data.ProductPicture = sponsoredContent.ProductPicture ?? data.ProductPicture;
            data.MinimumRequirement = sponsoredContent.MinimumRequirement ?? data.MinimumRequirement;
            data.Detail = sponsoredContent.Detail ?? data.Detail;
            data.PersonInCharge = sponsoredContent.PersonInCharge ?? data.PersonInCharge;
            data.ChannelTags = sponsoredContent.ChannelTags ?? data.ChannelTags;
            data.SectorTags = sponsoredContent.SectorTags ?? data.SectorTags;
            data.EndTime = sponsoredContent.EndTime ?? data.EndTime;
            data.FansNumberMinimum = sponsoredContent.FansNumberMinimum ?? data.FansNumberMinimum;
            db.SaveChanges();
            return Ok(data.ScId);
        }

        //提前關閉業配
        [HttpPut]
        [Route("api/CloseSponsoredContent/{id}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutSponsoredContentToClosed(int id)
        {
            SponsoredContent data = db.SponsoredContents.FirstOrDefault(f => f.ScId == id);
            if (data == null)
            {
                return NotFound();
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (currentUser.UserId != data.CompanyId)
            {
                return BadRequest("No Permission");
            }
            data.SCstatus = 0;
            db.SaveChanges();
            return Ok("Closed");
        }

        // POST: api/SponsoredContents
        [HttpPost]
        [Route("api/PostSponsoredContent")]
        [ResponseType(typeof(SponsoredContent))]
        public IHttpActionResult PostSponsoredContent(SponsoredContent sponsoredContent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(company))
            {
                return BadRequest("No Permission");
            }
            SponsoredContent sc = new SponsoredContent
            {
                Title = sponsoredContent.Title,
                Budget = sponsoredContent.Budget,
                PeopleRequired = sponsoredContent.PeopleRequired,
                ProductPicture = sponsoredContent.ProductPicture,
                MinimumRequirement = sponsoredContent.MinimumRequirement,
                FansNumberMinimum = sponsoredContent.FansNumberMinimum,
                Detail = sponsoredContent.Detail,
                PersonInCharge = sponsoredContent.PersonInCharge,
                ChannelTags = sponsoredContent.ChannelTags,
                SectorTags = sponsoredContent.SectorTags,
                EndTime = sponsoredContent.EndTime,
                CompanyId = currentUser.UserId,
                SCstatus = 1
            };
            db.SponsoredContents.Add(sc);
            db.SaveChanges();
            return Ok(sc.ScId);
        }

        // DELETE: api/SponsoredContents/5
        [HttpDelete]
        [Route("api/DeleteSponsoredContent/{id}")]
        [ResponseType(typeof(SponsoredContent))]
        public IHttpActionResult DeleteSponsoredContent(int id)
        {
            SponsoredContent sponsoredContent = db.SponsoredContents.FirstOrDefault(f => f.ScId == id);
            if (sponsoredContent == null)
            {
                return NotFound();
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (currentUser.UserId != sponsoredContent.CompanyId)
            {
                return BadRequest("No Permission");
            }
            if (sponsoredContent.Coops.Where(w => w.Status == (int)Status.雙方確認).Any())
            {
                return BadRequest("不能刪這個，有已經完成媒合的紀錄");
            }
            db.SponsoredContents.Remove(sponsoredContent);
            db.SaveChanges();
            return Ok("deleted");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private IQueryable<SponsoredContent> FilterByTags(IQueryable<SponsoredContent> sponsoredContents, SearchTags searchTags)
        {
            if (searchTags != null)
            {
                if (!string.IsNullOrEmpty(searchTags.ChannelTags) && !string.IsNullOrEmpty(searchTags.SectorTags))
                {
                    string[] channels = searchTags.ChannelTags.Split(',');
                    string[] sectors = searchTags.SectorTags.Split(',');
                    sponsoredContents = sponsoredContents.Where(x => channels.Any(y => x.ChannelTags.Contains(y)));
                    sponsoredContents = sponsoredContents.Where(x => sectors.Any(y => x.SectorTags.Contains(y)));
                }
                else if (!string.IsNullOrEmpty(searchTags.ChannelTags))
                {
                    string[] channels = searchTags.ChannelTags.Split(',');
                    sponsoredContents = sponsoredContents.Where(x => channels.Any(y => x.ChannelTags.Contains(y)));
                }
                else if (!string.IsNullOrEmpty(searchTags.SectorTags))
                {
                    string[] sectors = searchTags.SectorTags.Split(',');
                    sponsoredContents = sponsoredContents.Where(x => sectors.Any(y => x.SectorTags.Contains(y)));
                }
            }
            return sponsoredContents;
        }
    }
}