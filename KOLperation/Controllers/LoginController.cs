using KOLperation.Models;
using KOLperation.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Script.Serialization;
using static KOLperation.Utils.Enum;

namespace KOLperation.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LoginController : ApiController
    {
        private readonly AModel _db = new AModel();
        private readonly JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
        private readonly string kol = ConfigurationManager.AppSettings["iamkol"].ToString();
        private readonly string url = ConfigurationManager.AppSettings["ourUploadUrl"].ToString();
        protected string GoogleClientId = ConfigurationManager.AppSettings["GoogleClientId"].ToString();
        protected string GoogleClientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"].ToString();
        protected string GoogleRedirectUrl = "https://kolperation.rocket-coding.com/test.html";
        //protected string GoogleRedirectUrl = "https://kolperation.rocket-coding.com/index.html/firmplat/msg";

        [HttpGet]
        [Route("api/GoogleLogin")]
        public string GoogleLogin()
        {
            string guid = Guid.NewGuid().ToString();
            string guid2 = Guid.NewGuid().ToString();
            string GoogleUrl = String.Format("https://accounts.google.com/o/oauth2/v2/auth/identifier?response_type=code&prompt=select_account&scope=openid%20profile%20email&client_id={0}&redirect_uri={1}&nonce={2}&state={3}", GoogleClientId, GoogleRedirectUrl, guid, guid2);
            HttpResponseMessage responseMessage = Request.CreateResponse(HttpStatusCode.Moved);
            responseMessage.Headers.Location = new Uri(GoogleUrl);
            return GoogleUrl;
        }

        [HttpPost]
        [Route("api/RequestGoogleUserInfo")]
        public IHttpActionResult PostGoogleUserInfo([FromBody] Login login)
        {
            if (!login.state.Equals(login.stateReturnedByGoogle)) return BadRequest("你不是我");
            string url = "https://accounts.google.com/o/oauth2/token";
            string poststring = "grant_type=authorization_code&code=" + login.code + "&client_id=" + GoogleClientId + "&client_secret=" + GoogleClientSecret + "&redirect_uri=" + GoogleRedirectUrl + "";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            UTF8Encoding utfenc = new UTF8Encoding();
            byte[] bytes = utfenc.GetBytes(poststring);
            Stream outputstream = null;
            try
            {
                request.ContentLength = bytes.Length;
                outputstream = request.GetRequestStream();
                outputstream.Write(bytes, 0, bytes.Length);
            }
            catch { return BadRequest("Google登入失敗"); }
            var response = (HttpWebResponse)request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream());
            string responseFromServer = streamReader.ReadToEnd();
            JavaScriptSerializer js = new JavaScriptSerializer();
            Tokenclass obj = js.Deserialize<Tokenclass>(responseFromServer);
            Userclass userInfo = GetUserProfile(obj.access_token);
            string token;
            if (login.character == (int)Character.KOL)
            {
                UserKOL KOLuser = _db.UserKOLs.FirstOrDefault(user => user.Email == userInfo.email && user.Username == userInfo.name);
                if (KOLuser == null)
                {
                    string guid = Guid.NewGuid().ToString();
                    UserKOL newKol = new UserKOL
                    {
                        Guid = guid,
                        AccountId = "GoogleLoginUser",
                        Username = userInfo.name,
                        Avatar = userInfo.picture,
                        Email = userInfo.email,
                        JoinedDate = DateTime.Now,
                        Enabled = 1
                    };
                    _db.UserKOLs.Add(newKol);
                    _db.SaveChanges();
                    token = jwtAuthUtil.GenerateKOLToken(newKol);
                    return Ok(new
                    {
                        Name = newKol.Username,
                        Avatar = newKol.Avatar,
                        Character = login.character,
                        Check = 0,
                        IsGoogleUser = UserType.新的Google登入用戶,
                        Token = token
                    });
                }
                else if (String.IsNullOrEmpty(KOLuser.Password))
                {
                    token = jwtAuthUtil.GenerateKOLToken(KOLuser);
                    return Ok(new
                    {
                        Name = KOLuser.Username,
                        Avatar = KOLuser.Avatar,
                        Character = login.character,
                        Check = 0,
                        IsGoogleUser = UserType.已經註冊過Google的登入用戶,
                        Token = token
                    });
                }
                else
                {
                    return BadRequest("已註冊過");
                }
            }
            else if (login.character == (int)Character.公司)
            {
                UserCompany CompanyUser = _db.UserCompanies.FirstOrDefault(user => user.Email == userInfo.email && user.Company == userInfo.name);
                if (CompanyUser == null)
                {
                    string guid = Guid.NewGuid().ToString();
                    UserCompany newCompany = new UserCompany
                    {
                        Guid = guid,
                        AccountId = "GoogleLoginUser",
                        Company = userInfo.name,
                        CompanyLogo = userInfo.picture,
                        Email = userInfo.email,
                        JoinedDate = DateTime.Now,
                        Enabled = 1
                    };
                    _db.UserCompanies.Add(newCompany);
                    _db.SaveChanges();
                    token = jwtAuthUtil.GenerateCompanyToken(newCompany);
                    return Ok(new
                    {
                        Name = newCompany.Company,
                        Avatar = newCompany.CompanyLogo,
                        Character = login.character,
                        Check = 0,
                        IsGoogleUser = UserType.新的Google登入用戶,
                        Token = token
                    });
                }
                else if (String.IsNullOrEmpty(CompanyUser.Password))
                {
                    token = jwtAuthUtil.GenerateCompanyToken(CompanyUser);
                    return Ok(new
                    {
                        Name = CompanyUser.Company,
                        Avatar = CompanyUser.CompanyLogo,
                        Character = login.character,
                        Check = 0,
                        IsGoogleUser = UserType.已經註冊過Google的登入用戶,
                        Token = token
                    });
                }
                else
                {
                    return BadRequest("已註冊過");
                }
            }
            return BadRequest("無法辨識的人設");
        }

        private Userclass GetUserProfile(string accesstoken)
        {
            string url = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token=" + accesstoken + "";
            WebRequest request = WebRequest.Create(url);
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            JavaScriptSerializer js = new JavaScriptSerializer();
            Userclass userinfo = js.Deserialize<Userclass>(responseFromServer);
            return userinfo;
        }

        [HttpPost]
        [Route("api/Login")]
        public IHttpActionResult Login([FromBody] Login login)
        {
            string username, avatar, token;
            int character, checkPassword;
            UserKOL userKOL = _db.UserKOLs.FirstOrDefault(f => f.AccountId.Equals(login.Account));
            if (userKOL == null)
            {
                UserCompany userCompany = _db.UserCompanies.FirstOrDefault(f => f.AccountId.Equals(login.Account));
                if (userCompany == null)
                {
                    return BadRequest("nope");
                }
                else
                {
                    if (userCompany.Enabled == 0)
                    {
                        return BadRequest("nope");
                    }
                    if (userCompany.Password.EndsWith("ahahajisdf42"))
                    {
                        string TempPassword = PasswordSalt.GenerateHashWithSalt(login.Password, userCompany.PasswordSalt);
                        if (!userCompany.Password.Equals(TempPassword + "ahahajisdf42"))
                        {
                            return BadRequest("nope");
                        }
                        checkPassword = 1;
                    }
                    else
                    {
                        string DataPassword = PasswordSalt.GenerateHashWithSalt(login.Password, userCompany.PasswordSalt);
                        if (!userCompany.Password.Equals(DataPassword))
                        {
                            return BadRequest("nope");
                        }
                        checkPassword = 0;
                    }
                    username = userCompany.Company;
                    avatar = userCompany.CompanyLogo;
                    character = (int)Character.公司;
                    token = jwtAuthUtil.GenerateCompanyToken(userCompany);
                }
            }
            else
            {
                if (userKOL.Enabled == 0)
                {
                    return BadRequest("nope");
                }
                if (userKOL.Password.EndsWith("ahahajisdf42"))
                {
                    string TempPassword = PasswordSalt.GenerateHashWithSalt(login.Password, userKOL.PasswordSalt);
                    if (!userKOL.Password.Equals(TempPassword + "ahahajisdf42"))
                    {
                        return BadRequest("nope");
                    }
                    checkPassword = 1;
                }
                else
                {
                    string DataPassword = PasswordSalt.GenerateHashWithSalt(login.Password, userKOL.PasswordSalt);
                    if (!userKOL.Password.Equals(DataPassword))
                    {
                        return BadRequest("nope");
                    }

                    checkPassword = 0;
                }
                username = userKOL.Username;
                avatar = userKOL.Avatar;
                character = (int)Character.KOL;
                token = jwtAuthUtil.GenerateKOLToken(userKOL);
            }
            return Ok(new
            {
                Name = username,
                Avatar = avatar,
                Character = character,
                Check = checkPassword,
                IsGoogleUser = UserType.不是Google登入用戶,
                Token = token
            });
        }

        [HttpGet]
        [Route("api/GetCasesNum")]
        public IHttpActionResult GetCasesNum()
        {
            CurrentUser currentUser = JwtAuthFilter.GetPermission(Request.Headers.Authorization.Parameter);
            int onGoingCases, successfulCases;
            if (currentUser.Role.Equals(kol))
            {
                UserKOL userKOL = _db.UserKOLs.FirstOrDefault(u => u.KolId == currentUser.UserId);
                onGoingCases = userKOL.Coops.Where(c => c.Status == (int)Status.等KOL確認 || c.Status == (int)Status.等公司確認).Count();
                successfulCases = userKOL.Coops.Where(c => c.Status == (int)Status.雙方確認).Count();
            }
            else
            {
                var companyScs = _db.SponsoredContents.Where(w => w.CompanyId == currentUser.UserId);
                onGoingCases = companyScs.Where(ww => ww.SCstatus == 1).Count();
                successfulCases = companyScs.Where(ww => ww.SCstatus == 0).Count();
                //var companyCoops = _db.Coops.Where(w => w.SponsoredContent.CompanyId == currentUser.UserId);
                //onGoingCases = companyCoops.Where(w => w.Status == (int)Status.等KOL確認 || w.Status == (int)Status.等公司確認).Count();
                //successfulCases = companyCoops.Where(w => w.Status == (int)Status.雙方確認).Count();
            }
            return Ok(new
            {
                OnGoingCases = onGoingCases,
                SuccessfulCases = successfulCases,
            });
        }

        [HttpPost]
        [AcceptVerbs("GET", "POST")]
        [AllowAnonymous]
        [Route("OpenAccount")]
        public HttpResponseMessage OpenAccount([FromUri] AccountOpen accountOpen)
        {
            if (accountOpen.Ch.Equals("c"))
            {
                UserCompany userCompany = _db.UserCompanies.FirstOrDefault(f => f.Guid.Substring(2, 18).Equals(accountOpen.Gu) && f.AccountId.Equals(accountOpen.il));
                if (userCompany.Enabled == 0)
                {
                    userCompany.Enabled = 1;
                    _db.SaveChanges();
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Moved);
                    response.Headers.Location = new Uri("https://kolperation.rocket-coding.com/index.html#/login");
                    return response;
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.Forbidden);
                }
            }
            else
            {
                UserKOL userKOL = _db.UserKOLs.FirstOrDefault(f => f.Guid.Substring(2, 18).Equals(accountOpen.Gu) && f.AccountId.Equals(accountOpen.il));
                if (userKOL.Enabled == 0)
                {
                    userKOL.Enabled = 1;
                    _db.SaveChanges();
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Moved);
                    response.Headers.Location = new Uri("https://kolperation.rocket-coding.com/index.html#/login");
                    return response;
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.Forbidden);
                }
            }
        }

        [HttpPost]
        [Route("api/SendEmail")]
        public IHttpActionResult SendEmail([FromBody] CurrentUser user)
        {
            UserKOL kolUser = _db.UserKOLs.FirstOrDefault(f => f.Email.Equals(user.Email));
            if (kolUser == null)
            {
                UserCompany companyUser = _db.UserCompanies.FirstOrDefault(f => f.Email.Equals(user.Email));
                if (companyUser == null)
                {
                    return BadRequest("此Email沒有註冊的紀錄");
                }
                else
                {
                    if (!companyUser.Password.EndsWith("ahahajisdf42"))
                    {
                        string tempGuid = Guid.NewGuid().ToString();
                        string tempPassword = PasswordSalt.GenerateHashWithSalt(tempGuid, companyUser.PasswordSalt) + "ahahajisdf42";
                        companyUser.Password = tempPassword;
                        _db.SaveChanges();
                        try
                        {
                            string emailBody = Utility.PopulateBody(companyUser.Company, $@"請使用暫時的密碼登入，登入後請記得修改密碼。<br /> 暫時的密碼:{tempGuid}", "https://kolperation.rocket-coding.com/index.html#/login", "重新登入");
                            Utility.SendHtmlFormattedEmail(companyUser.Email, "忘記密碼", emailBody);
                            return Ok("sent");
                        }
                        catch
                        {
                            return BadRequest("fail to send email");
                        }
                    }
                }
            }
            if (!kolUser.Password.EndsWith("ahahajisdf42"))
            {
                string tempGuid = Guid.NewGuid().ToString();
                string tempPassword = PasswordSalt.GenerateHashWithSalt(tempGuid, kolUser.PasswordSalt) + "ahahajisdf42";
                kolUser.Password = tempPassword;
                _db.SaveChanges();
                try
                {
                    string emailBody = Utility.PopulateBody(kolUser.Username, $@"請使用暫時的密碼登入，登入後請記得修改密碼。<br /> 暫時的密碼:{tempGuid}", "https://kolperation.rocket-coding.com/index.html#/login", "重新登入");
                    Utility.SendHtmlFormattedEmail(kolUser.Email, "忘記密碼", emailBody);
                    return Ok("sent");
                }
                catch
                {
                    return BadRequest("fail to send email");
                }
            }
            return BadRequest("已經重新申請過，請收信");
        }

        [HttpPost]
        [Route("api/UploadFile")]
        public IHttpActionResult UploadFile()
        {
            HttpRequest request = HttpContext.Current.Request;
            if (request.Files.Count < 1)
            {
                return BadRequest("沒有看到檔案");
            }
            if (request.Files.Count > 1)
            {
                return BadRequest("only one~");
            }
            HttpPostedFile postedFile = request.Files[0];
            string fileType = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.') + 1).ToLower();
            if (fileType != "jpeg" && fileType != "jpg" && fileType != "png" && fileType != "gif")
            {
                return BadRequest("denied");
            }
            string imageRenamed = DateTime.Now.ToString("yyyyMMddHHmm") + postedFile.FileName;
            string path = HttpContext.Current.Server.MapPath("/") + "Upload/Image/";
            string imgPath = path + imageRenamed;
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists) directoryInfo.Create();
            postedFile.SaveAs(imgPath);
            return Ok(url + imageRenamed);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}