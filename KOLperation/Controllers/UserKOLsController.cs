using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using KOLperation.Models;
using KOLperation.Utils;
using static KOLperation.Utils.Enum;
using System.Web.Http.Cors;
using System.Configuration;

namespace KOLperation.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UserKOLsController : ApiController
    {
        private readonly AModel db = new AModel();
        private readonly string iconUrl = ConfigurationManager.AppSettings["ourIconUrl"].ToString();
        private readonly string url = ConfigurationManager.AppSettings["ourUploadUrl"].ToString();
        private readonly string kol = ConfigurationManager.AppSettings["iamkol"].ToString();
        private readonly string company = ConfigurationManager.AppSettings["iamcompany"].ToString();

        // GET: api/UserKOLs
        [HttpGet]
        [Route("api/GetKOLsList")]
        public IHttpActionResult GetUserKOLs([FromUri] SearchTags searchTags)
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(company))
            {
                return BadRequest("No Permission");
            }
            var userKOLs = db.UserKOLs.Where(k => k.Enabled == 1);
            var filtered = FilterByTags(userKOLs, searchTags);
            return Ok(filtered.Select(x => new
            {
                x.Guid,
                x.KolId,
                x.Username,
                x.KOLProfile,
                KOLavatar = x.Avatar,
                CoopTimes = x.Coops.Select(y => y.SponsoredContent.CompanyId == currentUser.UserId && y.Status == (int)Status.雙方確認).Where(r => r == true).Count(),
                Favorite = db.CompanyFavoriteKOLs.FirstOrDefault(f => f.CompanyId == currentUser.UserId && f.KOLId == x.KolId) != null,
                Sectors = db.TagSectors.Where(s => x.SectorTags.Contains(s.TagId.ToString())).Select(st => new
                {
                    st.TagName,
                    st.TagIcon
                }),
                Channels = x.ChannelDetails.Select(ct => new
                {
                    ct.Url,
                    ct.FansNumber,
                    ct.TagChannel.TagName,
                    Icon = iconUrl + ct.TagChannel.TagIcon
                })
            }));
        }

        // GET: api/UserKOLs/5
        //公司視角
        [HttpGet]
        [Route("api/GetKOL/{id}")]
        [ResponseType(typeof(UserKOL))]
        public IHttpActionResult GetUserKOL(string id)
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(company))
            {
                return BadRequest("No Permission");
            }
            var userKOL = db.UserKOLs.Where(k => k.Guid.Equals(id) && k.Enabled == 1);
            if (!userKOL.Any())
            {
                return NotFound();
            }
            return Ok(userKOL.Select(user => new
            {
                user.KolId,
                user.Username,
                user.KOLProfile,
                user.Email,
                user.Phone,
                KOLavatar = user.Avatar,
                CoopTimes = user.Coops.Select(stat => stat.SponsoredContent.CompanyId == currentUser.UserId && stat.Status == (int)Status.雙方確認).Where(r => r == true).Count(),
                Favorite = db.CompanyFavoriteKOLs.FirstOrDefault(f => f.CompanyId == currentUser.UserId && f.KOLId == user.KolId) != null,
                Sectors = db.TagSectors.Where(s => user.SectorTags.Contains(s.TagId.ToString())).Select(st => new
                {
                    st.TagName,
                    st.TagIcon
                }),
                Channels = user.ChannelDetails.Select(ct => new
                {
                    ct.Url,
                    ct.FansNumber,
                    ct.TagChannel.TagName,
                    Icon = iconUrl + ct.TagChannel.TagIcon
                })
            }));
        }

        //KOL視角
        [HttpGet]
        [Route("api/GetKOLforEditing")]
        [ResponseType(typeof(UserKOL))]
        public IHttpActionResult GetUserKOL()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol))
            {
                return BadRequest("No Permission");
            }
            var user = db.UserKOLs.Where(k => k.KolId == currentUser.UserId && k.Enabled == 1);
            if (!user.Any())
            {
                return NotFound();
            }
            int checkPassword = 0;
            if (user.FirstOrDefault().Password.EndsWith("ahahajisdf42"))
            {
                checkPassword = 1;
            }
            return Ok(user.Select(s => new
            {
                Check = checkPassword,
                s.KolId,
                s.AccountId,
                s.Username,
                s.KOLProfile,
                s.Email,
                s.Phone,
                s.JoinedDate,
                KOLavatar = s.Avatar,
                Sectors = db.TagSectors.Select(st => new
                {
                    st.TagId,
                    st.TagName,
                    st.TagIcon,
                    booling = s.SectorTags != null && s.SectorTags.Contains(st.TagId.ToString())
                }),
                Channels = db.TagChannels.Select(ct => new
                {
                    ct.ChannelId,
                    ct.TagName,
                    FansNumber = s.ChannelDetails.FirstOrDefault(fc => fc.ChannelId == ct.ChannelId && fc.KolId == currentUser.UserId) == null ? -1 : s.ChannelDetails.FirstOrDefault(fc => fc.ChannelId == ct.ChannelId && fc.KolId == currentUser.UserId).FansNumber,
                    Url = s.ChannelDetails.FirstOrDefault(fc => fc.ChannelId == ct.ChannelId && fc.KolId == currentUser.UserId) == null ? "null" : s.ChannelDetails.FirstOrDefault(fc => fc.ChannelId == ct.ChannelId && fc.KolId == currentUser.UserId).Url,
                    Icon = iconUrl + ct.TagIcon,
                    booling = s.ChannelDetails.FirstOrDefault(fd => fd.ChannelId == ct.ChannelId && fd.KolId == currentUser.UserId) != null
                })
            }));
            //    Sectors = db.TagSectors.Where(st => s.SectorTags.Contains(st.TagId.ToString())).Select(st => new
            //    {
            //        st.TagName,
            //        st.TagColor,
            //        Icon = iconUrl + st.TagIcon
            //    }),
            //    Channels = s.ChannelDetails.Select(ct => new
            //    {
            //        ct.Url,
            //        ct.FansNumber,
            //        ct.TagChannel.TagName,
            //        Icon = iconUrl + ct.TagChannel.TagIcon
            //    })
        }

        // POST: api/UserKOLs
        [HttpPost]
        [Route("api/KOLRegister")]
        [ResponseType(typeof(UserKOL))]
        public IHttpActionResult PostUserKOL(UserKOL userKOL)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (db.UserKOLs.FirstOrDefault(user => user.AccountId == userKOL.AccountId) != null
                || db.UserCompanies.FirstOrDefault(user => user.AccountId == userKOL.AccountId) != null)
            {
                return BadRequest("Duplicate");
            }
            if (String.IsNullOrEmpty(userKOL.AccountId)
               || String.IsNullOrEmpty(userKOL.Username)
               || String.IsNullOrEmpty(userKOL.Password)
               || String.IsNullOrEmpty(userKOL.Email)
               || String.IsNullOrEmpty(userKOL.Phone))
            {
                return BadRequest("都必填~");
            }
            //if (db.UserKOLs.FirstOrDefault(user => user.Email == userKOL.Email) != null
            //   || db.UserCompanies.FirstOrDefault(user => user.Email == userKOL.Email) != null)
            //{
            //    return BadRequest("Duplicate e");
            //}
            string guid = Guid.NewGuid().ToString();
            string salt = PasswordSalt.CreateSalt();
            Random rnd = new Random();
            int num = rnd.Next(10);
            UserKOL newKol = new UserKOL
            {
                Guid = guid,
                AccountId = userKOL.AccountId,
                Password = PasswordSalt.GenerateHashWithSalt(userKOL.Password, salt),
                PasswordSalt = salt,
                Username = userKOL.Username,
                Avatar = $@"{url}avatar{num}.png",
                KOLProfile = userKOL.KOLProfile,
                Email = Utility.IsValidEmail(userKOL.Email) ? userKOL.Email : "incorrectInput",
                Phone = Utility.IsValidCellnumber(userKOL.Phone) ? userKOL.Phone : "incorrectInput",
                ChannelDetails = userKOL.ChannelDetails,
                JoinedDate = DateTime.Now,
                Enabled = 0
            };
            if (newKol.Email == "incorrectInput" || newKol.Phone == "incorrectInput")
            {
                return BadRequest("Incorrect Input");
            }
            db.UserKOLs.Add(newKol);
            db.SaveChanges();
            try
            {
                string emailBody = Utility.PopulateBody(newKol.Username, "謝謝您的註冊，請點擊以下連結完成帳號開啟動作。", String.Format("https://kolperation.rocket-coding.com/OpenAccount/?Gu={0}&Ch=k&il={1}", newKol.Guid.Substring(2, 18), newKol.AccountId), "開啟帳號");
                Utility.SendHtmlFormattedEmail(newKol.Email, "開啟註冊帳號", emailBody);
                return Ok("Created");
            }
            catch (Exception)
            {
                return BadRequest("fail to send email");
            }
        }

        // PUT: api/UserKOLs/5
        [HttpPut]
        [Route("api/PutKOL/{id}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUserKOL(int id, UserKOL userKOL)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != userKOL.KolId)
            {
                return BadRequest("Refused");
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (currentUser.UserId != id)
            {
                return BadRequest("No Permission");
            }
            UserKOL data = db.UserKOLs.FirstOrDefault(f => f.KolId == id);
            if (data != null)
            {
                data.Username = userKOL.Username ?? data.Username;
                data.Avatar = userKOL.Avatar ?? url + data.Avatar;
                data.KOLProfile = userKOL.KOLProfile ?? data.KOLProfile;
                data.SectorTags = userKOL.SectorTags ?? data.SectorTags;
                if (userKOL.Email != null)
                {
                    if (Utility.IsValidEmail(userKOL.Email))
                    {
                        data.Email = userKOL.Email;
                    }
                    else
                    {
                        return BadRequest("Incorrect Input");
                    }
                }
                if (userKOL.Phone != null)
                {
                    if (Utility.IsValidCellnumber(userKOL.Phone))
                    {
                        data.Phone = userKOL.Phone;
                    }
                    else
                    {
                        return BadRequest("Incorrect Input");
                    }
                }
                if (userKOL.ChannelDetails != null)
                {
                    if (userKOL.ChannelDetails.Any())
                    {
                        var existingChannels = db.UserKOLChannelDetails.Where(d => d.KolId == id);
                        db.UserKOLChannelDetails.RemoveRange(existingChannels);
                        data.ChannelDetails = userKOL.ChannelDetails;
                    }
                }
                db.SaveChanges();
                return Ok("Updated");
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPut]
        [Route("api/PutKOLPass/{id}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUserKOLPass(int id, [FromBody] PasswordChange passwordChange)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (passwordChange == null)
            {
                return BadRequest("user didnt input anything");
            }
            if (id != passwordChange.UserId)
            {
                return BadRequest("Refused");
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (currentUser.UserId != id)
            {
                return BadRequest("No Permission");
            }
            UserKOL data = db.UserKOLs.FirstOrDefault(f => f.KolId == currentUser.UserId);
            if (data != null)
            {
                //if (!string.IsNullOrEmpty(passwordChange.OldPassword) && !string.IsNullOrEmpty(passwordChange.NewPassword) && !string.IsNullOrEmpty(passwordChange.NewPasswordConfirmation))
                //{
                //    if (!data.Password.Equals(PasswordSalt.GenerateHashWithSalt(passwordChange.OldPassword, data.PasswordSalt)))
                //    {
                //        return BadRequest("wrong old pass");
                //    }
                //    if (!passwordChange.NewPassword.Equals(passwordChange.NewPasswordConfirmation))
                //    {
                //        return BadRequest("not equal oh");
                //    }
                if (!string.IsNullOrEmpty(passwordChange.NewPassword))
                {
                    data.PasswordSalt = PasswordSalt.CreateSalt();
                    data.Password = PasswordSalt.GenerateHashWithSalt(passwordChange.NewPassword, data.PasswordSalt);
                    db.SaveChanges();
                    return Ok("Updated");
                }
                //}
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/UserKOLs/5
        [HttpDelete]
        [Route("api/DeleteKOLAccount/{id}")]
        [ResponseType(typeof(UserKOL))]
        public IHttpActionResult DeleteUserKOL(int id)
        {
            UserKOL userKOL = db.UserKOLs.FirstOrDefault(f => f.KolId == id);
            if (userKOL == null)
            {
                return NotFound();
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (currentUser.UserId != id)
            {
                return BadRequest("No Permission");
            }
            if (userKOL.KolId != id)
            {
                return BadRequest("Refused");
            }
            userKOL.Enabled = 0;
            userKOL.AccountId += "_Closed";
            userKOL.Email += "_Closed";
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

        private IQueryable<UserKOL> FilterByTags(IQueryable<UserKOL> userKOLs, SearchTags searchTags)
        {
            if (searchTags != null)
            {
                if (!string.IsNullOrEmpty(searchTags.ChannelTags) && !string.IsNullOrEmpty(searchTags.SectorTags) && !string.IsNullOrEmpty(searchTags.Fans))
                {
                    int[] channels = searchTags.ChannelTags.Split(',').Select(int.Parse).ToArray();
                    userKOLs = userKOLs.Where(x => channels.Any(y => x.ChannelDetails.Any(c => c.ChannelId == y)));

                    string[] sectors = searchTags.SectorTags.Split(',');
                    userKOLs = userKOLs.Where(x => sectors.Any(y => x.SectorTags.Contains(y)));

                    string[] fans = searchTags.Fans.Split(',');
                    foreach (string numStr in fans)
                    {
                        string[] num = numStr.Split('~');
                        int min = Int32.Parse(num[0]);
                        int max = Int32.Parse(num[1]);
                        userKOLs = userKOLs.Where(x => x.ChannelDetails.Any(y => y.FansNumber >= min && y.FansNumber <= max));
                    }
                }
                else if (!string.IsNullOrEmpty(searchTags.ChannelTags) && !string.IsNullOrEmpty(searchTags.Fans))
                {
                    int[] channels = searchTags.ChannelTags.Split(',').Select(int.Parse).ToArray();
                    userKOLs = userKOLs.Where(x => channels.Any(y => x.ChannelDetails.Any(c => c.ChannelId == y)));

                    string[] fans = searchTags.Fans.Split(',');
                    foreach (string numStr in fans)
                    {
                        string[] num = numStr.Split('~');
                        int min = Int32.Parse(num[0]);
                        int max = Int32.Parse(num[1]);
                        userKOLs = userKOLs.Where(x => x.ChannelDetails.Any(y => y.FansNumber >= min && y.FansNumber <= max));
                    }
                }
                else if (!string.IsNullOrEmpty(searchTags.SectorTags) && !string.IsNullOrEmpty(searchTags.Fans))
                {
                    string[] sectors = searchTags.SectorTags.Split(',');
                    userKOLs = userKOLs.Where(x => sectors.Any(y => x.SectorTags.Contains(y)));

                    string[] fans = searchTags.Fans.Split(',');
                    foreach (string numStr in fans)
                    {
                        string[] num = numStr.Split('~');
                        int min = Int32.Parse(num[0]);
                        int max = Int32.Parse(num[1]);
                        userKOLs = userKOLs.Where(x => x.ChannelDetails.Any(y => y.FansNumber >= min && y.FansNumber <= max));
                    }
                }
                else if (!string.IsNullOrEmpty(searchTags.ChannelTags) && !string.IsNullOrEmpty(searchTags.SectorTags))
                {
                    int[] channels = searchTags.ChannelTags.Split(',').Select(int.Parse).ToArray();
                    userKOLs = userKOLs.Where(x => channels.Any(y => x.ChannelDetails.Any(c => c.ChannelId == y)));

                    string[] sectors = searchTags.SectorTags.Split(',');
                    userKOLs = userKOLs.Where(x => sectors.Any(y => x.SectorTags.Contains(y)));
                }
                else if (!string.IsNullOrEmpty(searchTags.ChannelTags))
                {
                    int[] channels = searchTags.ChannelTags.Split(',').Select(int.Parse).ToArray();
                    userKOLs = userKOLs.Where(x => channels.Any(y => x.ChannelDetails.Any(c => c.ChannelId == y)));
                }
                else if (!string.IsNullOrEmpty(searchTags.SectorTags))
                {
                    string[] sectors = searchTags.SectorTags.Split(',');
                    userKOLs = userKOLs.Where(x => sectors.Any(y => x.SectorTags.Contains(y)));
                }
                else if (!string.IsNullOrEmpty(searchTags.Fans))
                {
                    string[] fans = searchTags.Fans.Split(',');
                    foreach (string numStr in fans)
                    {
                        string[] num = numStr.Split('~');
                        int min = Int32.Parse(num[0]);
                        int max = Int32.Parse(num[1]);
                        userKOLs = userKOLs.Where(x => x.ChannelDetails.Any(y => y.FansNumber >= min && y.FansNumber <= max));
                    }
                }
            }
            return userKOLs;
        }
    }
}