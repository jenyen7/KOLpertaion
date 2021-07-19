using Jose;
using KOLperation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Filters;

namespace KOLperation.Utils
{
    public class JwtAuthFilter : ActionFilterAttribute
    {
        private const string secretKey = "kkkcccPlat";//加解密的key,如果不一樣會無法成功解密

        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var request = actionContext.Request;
            if (!WithoutVerifyToken(request.RequestUri.ToString()))
            {
                if (request.Headers.Authorization == null || request.Headers.Authorization.Scheme != "Bearer")
                {
                    var errorMessage = new HttpResponseMessage()
                    {
                        ReasonPhrase = "Lost Token",
                        Content = new StringContent("code=2414")
                    };
                    //throw new HttpResponseException(errorMessage);
                    //throw new System.Exception("Lost Token");
                    throw new Exception("Lost Token");
                }
                else
                {
                    try
                    {
                        //解密後會回傳Json格式的物件(即加密前的資料)
                        var jwtObject = JWT.Decode<Dictionary<string, Object>>(
                        request.Headers.Authorization.Parameter,
                        Encoding.UTF8.GetBytes(secretKey),
                        JwsAlgorithm.HS512);

                        if (IsTokenExpired(jwtObject["Exp"].ToString()))
                        {
                            var errorMessage = new HttpResponseMessage()
                            {
                                ReasonPhrase = "Token Expired",
                                Content = new StringContent("code=2415")
                            };
                            //throw new HttpResponseException(errorMessage);
                            //throw new System.Exception("Token Expired");
                            throw new Exception("Token Expired");
                        }
                    }
                    catch (Exception)
                    {
                        var errorMessage = new HttpResponseMessage()
                        {
                            ReasonPhrase = "Unrecognizable Token",
                            Content = new StringContent($"code=2416")
                        };
                        //throw new HttpResponseException(errorMessage);
                        throw new Exception("Unrecognizable Token");
                    }
                }
            }
            base.OnActionExecuting(actionContext);
        }

        //Login不需要驗證因為還沒有token(全部網頁都要驗證，除了下列幾頁)
        public bool WithoutVerifyToken(string requestUri)
        {
            if (requestUri.EndsWith("api/Login")
                || requestUri.EndsWith("api/GetGoogleLink")
                || requestUri.Contains("api/GetGoogleUserInfo")
                || requestUri.Contains("/OpenAccount")
                || requestUri.EndsWith("/SendEmail")
                || requestUri.EndsWith("api/KOLRegister")
                || requestUri.EndsWith("api/CompanyRegister")
                || requestUri.Contains("api/GetSponsoredContentsPreview")
                || requestUri.EndsWith("api/TagChannels")
                || requestUri.EndsWith("api/TagSectors"))
                return true;
            return false;
        }

        public bool IsTokenExpired(string dateTime)
        {
            return Convert.ToDateTime(dateTime) < DateTime.Now;
        }

        public static CurrentUser GetPermission(string token)
        {
            var tokenData = JWT.Decode<Dictionary<string, object>>(token, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            CurrentUser currentUser = new CurrentUser
            {
                Role = tokenData["Role"].ToString(),
                UserId = Convert.ToInt32((tokenData["Id"])),
                Email = tokenData["Permission"].ToString()
            };
            return currentUser;
        }
    }
}