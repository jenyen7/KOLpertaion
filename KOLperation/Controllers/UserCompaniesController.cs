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
    public class UserCompaniesController : ApiController
    {
        private readonly AModel db = new AModel();
        private readonly string iconUrl = ConfigurationManager.AppSettings["ourIconUrl"].ToString();
        private readonly string url = ConfigurationManager.AppSettings["ourUploadUrl"].ToString();
        private readonly string kol = ConfigurationManager.AppSettings["iamkol"].ToString();
        private readonly string company = ConfigurationManager.AppSettings["iamcompany"].ToString();

        // GET: api/UserCompanies
        [HttpGet]
        [Route("api/GetCompaniesList")]
        public IHttpActionResult GetUserCompanies()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol))
            {
                return BadRequest("No Permission");
            }
            return Ok(db.UserCompanies.Where(c => c.Enabled == 1).Select(x => new
            {
                x.Guid,
                x.Company,
                x.CompanyProfile,
                x.CompanyLogo,
                Channels = db.TagChannels.Where(c => x.ChannelTags.Contains(c.TagId.ToString())).Select(ct => new
                {
                    ct.FAid,
                    ct.TagName,
                    Icon = iconUrl + ct.TagIcon
                }),
                Sectors = db.TagSectors.Where(s => x.SectorTags.Contains(s.TagId.ToString())).Select(st => new
                {
                    st.TagName,
                    st.TagIcon
                }),
            }));
        }

        // GET: api/UserCompanies/5
        //KOL視角
        [HttpGet]
        [Route("api/GetCompany/{id}")]
        [ResponseType(typeof(UserCompany))]
        public IHttpActionResult GetUserCompany(string id)
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(kol))
            {
                return BadRequest("No Permission");
            }
            UserCompany userCompany = db.UserCompanies.FirstOrDefault(c => c.Guid.Equals(id) && c.Enabled == 1);
            if (userCompany == null)
            {
                return NotFound();
            }
            return Ok(new
            {
                userCompany.Company,
                userCompany.CompanyProfile,
                userCompany.Website,
                userCompany.Email,
                userCompany.Phone,
                userCompany.Cellphone,
                userCompany.Address,
                userCompany.PersonInCharge,
                userCompany.CompanyLogo,
                //CoopSuccessTimes = userCompany.SponsoredContents.Select(co => co.Coops.Select(stat => stat.Status == (int)Status.雙方確認)).Where(r => r.Contains(true)).Count(),
                CoopSuccessTimes = db.Coops.Where(coop => coop.SponsoredContent.CompanyId == userCompany.ComId && coop.Status == (int)Status.雙方確認).Count(),
                Favorite = db.KOLFavoriteCompanies.FirstOrDefault(f => f.CompanyId == userCompany.ComId && f.KOLId == currentUser.UserId) != null,
                Channels = db.TagChannels.Where(c => userCompany.ChannelTags.Contains(c.TagId.ToString())).Select(ct => new
                {
                    ct.FAid,
                    ct.TagName,
                    Icon = iconUrl + ct.TagIcon
                }),
                Sectors = db.TagSectors.Where(s => userCompany.SectorTags.Contains(s.TagId.ToString())).Select(st => new
                {
                    st.TagName,
                    st.TagIcon
                }),
            });
        }

        //公司視角
        [HttpGet]
        [Route("api/GetCompanyforEditing")]
        [ResponseType(typeof(UserCompany))]
        public IHttpActionResult GetUserCompany()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (!currentUser.Role.Equals(company))
            {
                return BadRequest("No Permission");
            }
            UserCompany user = db.UserCompanies.FirstOrDefault(c => c.ComId == currentUser.UserId && c.Enabled != 0);
            if (user == null)
            {
                return NotFound();
            }
            int checkPassword = 0;
            if (user.Password.EndsWith("ahahajisdf42"))
            {
                checkPassword = 1;
            }
            return Ok(new
            {
                Check = checkPassword,
                user.ComId,
                user.AccountId,
                user.TaxIdNumber,
                user.Company,
                user.CompanyProfile,
                user.Website,
                user.Email,
                user.Phone,
                user.Cellphone,
                user.Address,
                user.PersonInCharge,
                user.CompanyLogo,
                Sectors = db.TagSectors.Select(st => new
                {
                    st.TagId,
                    st.TagName,
                    st.TagIcon,
                    booling = user.SectorTags != null && user.SectorTags.Contains(st.TagId.ToString())
                }),
                Channels = db.TagChannels.Select(ct => new
                {
                    ct.TagId,
                    ct.FAid,
                    ct.TagName,
                    Icon = iconUrl + ct.TagIcon,
                    booling = user.ChannelTags != null && user.ChannelTags.Contains(ct.TagId.ToString())
                }),
            });
        }

        // PUT: api/UserCompanies/5
        [HttpPut]
        [Route("api/PutCompany/{id}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUserCompany(int id, UserCompany userCompany)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != userCompany.ComId)
            {
                return BadRequest("Refused");
            }
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            if (currentUser.UserId != id)
            {
                return BadRequest("No Permission");
            }
            UserCompany data = db.UserCompanies.FirstOrDefault(f => f.ComId == id);
            if (data != null)
            {
                data.Company = userCompany.Company ?? data.Company;
                data.CompanyLogo = userCompany.CompanyLogo ?? url + data.CompanyLogo;
                data.CompanyProfile = userCompany.CompanyProfile ?? data.CompanyProfile;
                data.Website = userCompany.Website ?? data.Website;
                data.Phone = userCompany.Phone ?? data.Phone;
                data.Address = userCompany.Address ?? data.Address;
                data.PersonInCharge = userCompany.PersonInCharge ?? data.PersonInCharge;
                data.ChannelTags = userCompany.ChannelTags ?? data.ChannelTags;
                data.SectorTags = userCompany.SectorTags ?? data.SectorTags;
                if (userCompany.Email != null)
                {
                    if (Utility.IsValidEmail(userCompany.Email))
                    {
                        data.Email = userCompany.Email;
                    }
                    else
                    {
                        return BadRequest("Incorrect Input");
                    }
                }
                if (userCompany.Cellphone != null)
                {
                    if (Utility.IsValidCellnumber(userCompany.Cellphone))
                    {
                        data.Cellphone = userCompany.Cellphone;
                    }
                    else
                    {
                        return BadRequest("Incorrect Input");
                    }
                }
                db.SaveChanges();
                return Ok("Updated");
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPut]
        [Route("api/PutCompanyPass/{id}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUserCompanyPass(int id, [FromBody] PasswordChange passwordChange)
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
            UserCompany data = db.UserCompanies.FirstOrDefault(f => f.ComId == currentUser.UserId);
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
                //        return BadRequest("pass not equal lah");
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

        // POST: api/UserCompanies
        [Route("api/CompanyRegister")]
        [ResponseType(typeof(UserCompany))]
        public IHttpActionResult PostUserCompany(UserCompany userCompany)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (db.UserCompanies.FirstOrDefault(user => user.AccountId == userCompany.AccountId) != null
                || db.UserKOLs.FirstOrDefault(user => user.AccountId == userCompany.AccountId) != null)
            {
                return BadRequest("Duplicate");
            }
            if (String.IsNullOrEmpty(userCompany.AccountId)
                || String.IsNullOrEmpty(userCompany.Company)
                || String.IsNullOrEmpty(userCompany.Password)
                || String.IsNullOrEmpty(userCompany.Email)
                || String.IsNullOrEmpty(userCompany.Cellphone)
                || String.IsNullOrEmpty(userCompany.TaxIdNumber))
            {
                return BadRequest("都必填~");
            }
            //if (db.UserKOLs.FirstOrDefault(user => user.Email == userCompany.Email) != null
            //   || db.UserCompanies.FirstOrDefault(user => user.Email == userCompany.Email) != null)
            //{
            //    return BadRequest("Duplicate e");
            //}
            string guid = Guid.NewGuid().ToString();
            string salt = PasswordSalt.CreateSalt();
            Random rnd = new Random();
            int num = rnd.Next(10);
            UserCompany newCompany = new UserCompany
            {
                Guid = guid,
                AccountId = userCompany.AccountId,
                Password = PasswordSalt.GenerateHashWithSalt(userCompany.Password, salt),
                PasswordSalt = salt,
                TaxIdNumber = Utility.IsValidTaxIdNumber(userCompany.TaxIdNumber) ? userCompany.TaxIdNumber : "incorrectInput",
                Company = userCompany.Company,
                CompanyLogo = $@"{url}avatar{num}.png",
                Email = Utility.IsValidEmail(userCompany.Email) ? userCompany.Email : "incorrectInput",
                Cellphone = Utility.IsValidCellnumber(userCompany.Cellphone) ? userCompany.Cellphone : "incorrectInput",
                ChannelTags = userCompany.ChannelTags,
                JoinedDate = DateTime.Now,
                Enabled = 0
            };
            if (newCompany.Email == "incorrectInput" || newCompany.Cellphone == "incorrectInput")
            {
                return BadRequest("Incorrect Input");
            }
            db.UserCompanies.Add(newCompany);
            db.SaveChanges();
            try
            {
                string emailBody = Utility.PopulateBody(newCompany.Company, "謝謝您的註冊，請點擊以下連結完成帳號開啟動作。", String.Format("https://kolperation.rocket-coding.com/OpenAccount/?Gu={0}&Ch=c&il={1}", newCompany.Guid.Substring(2, 18), newCompany.AccountId), "開啟帳號");
                Utility.SendHtmlFormattedEmail(newCompany.Email, "開啟註冊帳號", emailBody);
                return Ok("Created");
            }
            catch (Exception)
            {
                return BadRequest("fail to send email");
            }
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