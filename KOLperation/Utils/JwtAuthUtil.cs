using Jose;
using KOLperation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace KOLperation.Utils
{
    public class JwtAuthUtil
    {
        private const string secretKey = "kkkcccPlat";

        public string GenerateCompanyToken(UserCompany user)
        {
            Dictionary<string, Object> payload = new Dictionary<string, Object>
            {
                {"Role", "companyUserRights"},
                {"Id", user.ComId},
                {"Permission", user.Email},
                {"Exp", DateTime.Now.AddDays(1)}
            };
            string token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        public string GenerateKOLToken(UserKOL user)
        {
            Dictionary<string, Object> payload = new Dictionary<string, Object>
            {
                {"Role", "kolUserRights"},
                {"Id", user.KolId},
                {"Permission", user.Email},
                {"Exp", DateTime.Now.AddDays(1)}
            };
            string token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }
    }
}