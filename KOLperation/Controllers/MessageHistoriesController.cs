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
using Microsoft.AspNet.SignalR;
using static KOLperation.Utils.Enum;

namespace KOLperation.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MessageHistoriesController : ApiController
    {
        private readonly AModel db = new AModel();
        private readonly string kol = ConfigurationManager.AppSettings["iamkol"].ToString();
        private readonly string company = ConfigurationManager.AppSettings["iamcompany"].ToString();

        // GET: api/MessageHistories
        [HttpGet]
        [Route("api/GetMessageHistories")]
        public IHttpActionResult GetMessageHistories()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (currentUser.Role.Equals(kol))
            {
                return Ok(db.MessageHistories.Where(w => w.KolId == currentUser.UserId).Select(s => new
                {
                    s.MsgId,
                    s.SponsoredContentId,
                    Status = db.Coops.FirstOrDefault(coop => coop.SponsoredContentId == s.SponsoredContentId && coop.KOLId == currentUser.UserId) == null ? 42 : db.Coops.FirstOrDefault(coop => coop.SponsoredContentId == s.SponsoredContentId && coop.KOLId == currentUser.UserId).Status,
                    s.SponsoredContent.Title,
                    s.SponsoredContent.UserCompany.Company,
                    s.SponsoredContent.UserCompany.CompanyLogo,
                    s.SponsoredContent.PersonInCharge,
                    LastestMessage = s.MessageHistoryContents.Select(c => new { c.Message, c.MessageTime }).OrderByDescending(om => om.MessageTime).FirstOrDefault(),
                    Character = Character.KOL,
                    CurrentUserId = currentUser.UserId,
                    SponsoredContentStatus = s.SponsoredContent.SCstatus
                }).Where(w => w.Status != 2).OrderByDescending(o => o.LastestMessage.MessageTime));
            }
            else
            {
                return Ok(db.MessageHistories.Where(w => w.SponsoredContent.CompanyId == currentUser.UserId).Select(s => new
                {
                    s.MsgId,
                    s.SponsoredContentId,
                    Status = db.Coops.FirstOrDefault(coop => coop.KOLId == s.UserKOL.KolId && coop.SponsoredContentId == s.SponsoredContentId) == null ? 42 : db.Coops.FirstOrDefault(coop => coop.KOLId == s.UserKOL.KolId && coop.SponsoredContentId == s.SponsoredContentId).Status,
                    s.SponsoredContent.Title,
                    s.UserKOL.Username,
                    s.KolId,
                    KOLavatar = s.UserKOL.Avatar,
                    LastestMessage = s.MessageHistoryContents.Select(c => new { c.Message, c.MessageTime }).OrderByDescending(om => om.MessageTime).FirstOrDefault(),
                    Character = Character.公司,
                    CurrentUserId = currentUser.UserId,
                    SponsoredContentStatus = s.SponsoredContent.SCstatus
                }).Where(w => w.SponsoredContentStatus == 1).OrderByDescending(o => o.LastestMessage.MessageTime));
            }
        }

        // GET: api/MessageHistories/5
        [HttpGet]
        [Route("api/GetMessageHistory/{id}")]
        public IHttpActionResult GetMessageHistoryContent(int id)
        {
            var msgs = db.MessageHistories.Where(w => w.MsgId == id);
            if (!msgs.Any())
            {
                return NotFound();
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (currentUser.Role.Equals(kol))
            {
                if (msgs.FirstOrDefault(s => s.KolId == currentUser.UserId) == null)
                {
                    return BadRequest("No permission");
                }
                return Ok(msgs.Select(s => new
                {
                    s.MsgId,
                    CoopStatus = db.Coops.Where(ww => ww.SponsoredContentId == s.SponsoredContentId && ww.KOLId == currentUser.UserId).FirstOrDefault() == null ? 42 : db.Coops.Where(ww => ww.SponsoredContentId == s.SponsoredContentId && ww.KOLId == currentUser.UserId).FirstOrDefault().Status,
                    s.SponsoredContentId,
                    s.SponsoredContent.Title,
                    s.SponsoredContent.UserCompany.Company,
                    s.SponsoredContent.PersonInCharge,
                    s.SponsoredContent.UserCompany.CompanyLogo,
                    s.SponsoredContent.SCstatus,
                    Message = db.MessageHistoryContents.Where(w => w.MsgId == id).Select(x => new
                    {
                        x.MsgContentId,
                        x.Message,
                        x.MessageTime,
                        x.Sender
                    })
                }));
            }
            else
            {
                if (msgs.FirstOrDefault(s => s.SponsoredContent.CompanyId == currentUser.UserId) == null)
                {
                    return BadRequest("No permission");
                }
                return Ok(msgs.Select(s => new
                {
                    s.MsgId,
                    CoopStatus = db.Coops.Where(ww => ww.SponsoredContentId == s.SponsoredContentId && ww.MessageHistoryId == s.MsgId).FirstOrDefault() == null ? 42 : db.Coops.Where(ww => ww.SponsoredContentId == s.SponsoredContentId && ww.MessageHistoryId == s.MsgId).FirstOrDefault().Status,
                    s.SponsoredContentId,
                    s.SponsoredContent.Title,
                    s.KolId,
                    s.UserKOL.Username,
                    KOLavatar = s.UserKOL.Avatar,
                    s.SponsoredContent.SCstatus,
                    Message = db.MessageHistoryContents.Where(w => w.MsgId == id).Select(x => new
                    {
                        x.MsgContentId,
                        x.Message,
                        x.MessageTime,
                        x.Sender
                    })
                }));
            }
        }

        [HttpPost]
        [Route("api/PostMessagebyKOL")]
        [ResponseType(typeof(MessageHistory))]
        public IHttpActionResult PostMessageHistorybyKOL(MessageHistory messageHistory)
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol)) return BadRequest("No permission");
            Coop checkCoopPresence = db.Coops.FirstOrDefault(c => c.SponsoredContentId == messageHistory.SponsoredContentId && c.KOLId == currentUser.UserId);
            var checkMessagePresence = db.MessageHistories.Where(f => f.SponsoredContentId == messageHistory.SponsoredContentId && f.KolId == currentUser.UserId);
            if (!checkMessagePresence.Any())
            {
                MessageHistory message = new MessageHistory
                {
                    KolId = currentUser.UserId,
                    SponsoredContentId = messageHistory.SponsoredContentId
                };
                db.MessageHistories.Add(message);
                db.SaveChanges();
                if (checkCoopPresence != null)
                {
                    checkCoopPresence.MessageHistoryId = message.MsgId;
                    db.SaveChanges();
                }
                return Ok(db.MessageHistories.Where(w => w.SponsoredContentId == message.SponsoredContentId && w.KolId == message.KolId).Select(m => new
                {
                    m.MsgId,
                    m.SponsoredContentId,
                    m.SponsoredContent.Title,
                    m.SponsoredContent.UserCompany.Company,
                    m.SponsoredContent.UserCompany.CompanyLogo,
                    note = "開啟新的聊天室窗~"
                }));
            }
            else
            {
                return Ok(checkMessagePresence.Select(s => new
                {
                    s.MsgId,
                    s.SponsoredContentId,
                    s.SponsoredContent.Title,
                    s.SponsoredContent.UserCompany.Company,
                    s.SponsoredContent.UserCompany.CompanyLogo,
                    note = "他們有聊過了~(也有可能還沒講話但點擊過了)",
                    Message = db.MessageHistoryContents.Where(y => y.MsgId == s.MsgId).Select(x => new
                    {
                        x.Message,
                        x.MessageTime,
                        x.Sender
                    })
                }));
            }
        }

        [HttpPost]
        [Route("api/PostMessagebyCompany")]
        [ResponseType(typeof(MessageHistory))]
        public IHttpActionResult PostMessageHistorybyCompany(MessageHistory messageHistory)
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(company)) return BadRequest("No permission");
            SponsoredContent sponsoredContent = db.SponsoredContents.FirstOrDefault(f => f.ScId == messageHistory.SponsoredContentId);
            if (sponsoredContent == null) return BadRequest("do not exist");
            if (sponsoredContent.CompanyId != currentUser.UserId) return BadRequest("No permission");
            Coop checkCoopPresence = db.Coops.FirstOrDefault(c => c.SponsoredContentId == messageHistory.SponsoredContentId && c.KOLId == messageHistory.KolId);
            var checkMessagePresence = db.MessageHistories.Where(f => f.SponsoredContentId == messageHistory.SponsoredContentId && f.KolId == messageHistory.KolId);
            if (!checkMessagePresence.Any())
            {
                MessageHistory message = new MessageHistory
                {
                    KolId = messageHistory.KolId,
                    SponsoredContentId = messageHistory.SponsoredContentId
                };
                db.MessageHistories.Add(message);
                db.SaveChanges();
                if (checkCoopPresence != null)
                {
                    checkCoopPresence.MessageHistoryId = message.MsgId;
                    db.SaveChanges();
                }
                return Ok(db.MessageHistories.Where(w => w.SponsoredContentId == message.SponsoredContentId && w.KolId == message.KolId).Select(m => new
                {
                    m.MsgId,
                    m.KolId,
                    m.SponsoredContentId,
                    m.SponsoredContent.Title,
                    m.UserKOL.Username,
                    KOLavatar = m.UserKOL.Avatar,
                    note = "開啟新的聊天室窗~"
                }));
            }
            else
            {
                return Ok(checkMessagePresence.Select(s => new
                {
                    s.MsgId,
                    s.SponsoredContentId,
                    s.SponsoredContent.Title,
                    s.UserKOL.Username,
                    KOLavatar = s.UserKOL.Avatar,
                    note = "他們有聊過了~(也有可能還沒講話但點擊過了)",
                    Message = db.MessageHistoryContents.Where(y => y.MsgId == s.MsgId).Select(x => new
                    {
                        x.Message,
                        x.MessageTime,
                        x.Sender
                    })
                }));
            }
        }

        [HttpPost]
        [Route("api/ChatbyCompany")]
        public IHttpActionResult PostMessageByCompany(MessageHistoryContent messageHistoryContent)
        {
            //hub.Clients.All.companySendMsgAsync(messageHistoryContent.Message);
            MessageHistoryContent message = new MessageHistoryContent
            {
                MsgId = messageHistoryContent.MsgId,
                Sender = (int)Character.公司,
                Message = messageHistoryContent.Message,
                MessageTime = DateTime.Now
            };
            db.MessageHistoryContents.Add(message);
            db.SaveChanges();
            return Ok(db.MessageHistories.Where(w => w.MsgId == message.MsgId).Select(s => new
            {
                s.MsgId,
                s.SponsoredContentId,
                s.SponsoredContent.Title,
                s.UserKOL.Username,
                KOLavatar = s.UserKOL.Avatar,
                Message = db.MessageHistoryContents.Where(y => y.MsgId == s.MsgId).Select(x => new
                {
                    x.MsgContentId,
                    x.Message,
                    x.MessageTime,
                    x.Sender
                })
            }));
        }

        [HttpPost]
        [Route("api/ChatbyKOL")]
        public IHttpActionResult PostMessageByKOL(MessageHistoryContent messageHistoryContent)
        {
            //hub.Clients.All.kolSendMsgAsync(messageHistoryContent.Message);
            MessageHistoryContent message = new MessageHistoryContent
            {
                MsgId = messageHistoryContent.MsgId,
                Sender = (int)Character.KOL,
                Message = messageHistoryContent.Message,
                MessageTime = DateTime.Now
            };
            db.MessageHistoryContents.Add(message);
            db.SaveChanges();
            return Ok(db.MessageHistories.Where(w => w.MsgId == message.MsgId).Select(s => new
            {
                s.MsgId,
                s.SponsoredContentId,
                s.SponsoredContent.Title,
                s.SponsoredContent.UserCompany.Company,
                Message = db.MessageHistoryContents.Where(y => y.MsgId == s.MsgId).Select(x => new
                {
                    x.MsgContentId,
                    x.Message,
                    x.MessageTime,
                    x.Sender
                })
            }));
        }

        [HttpDelete]
        [Route("api/DeleteMessage/{id}")]
        [ResponseType(typeof(MessageHistory))]
        public IHttpActionResult DeleteMessageHistory(int id)
        {
            MessageHistoryContent messageHistoryContent = db.MessageHistoryContents.Find(id);
            if (messageHistoryContent == null)
            {
                return NotFound();
            }
            int MsgId = messageHistoryContent.MsgId;
            db.MessageHistoryContents.Remove(messageHistoryContent);
            db.SaveChanges();
            return Ok(db.MessageHistories.Where(w => w.MsgId == MsgId).Select(s => new
            {
                s.MsgId,
                s.SponsoredContentId,
                s.SponsoredContent.Title,
                s.SponsoredContent.UserCompany.Company,
                s.UserKOL.Username,
                Message = db.MessageHistoryContents.Where(y => y.MsgId == s.MsgId).Select(x => new
                {
                    x.MsgContentId,
                    x.Message,
                    x.MessageTime,
                    x.Sender
                })
            }));
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