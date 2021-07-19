using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KOLperation.Models
{
    public class Login
    {
        [Required]
        [Display(Name = "帳號")]
        public string Account { get; set; }

        [Required]
        [Display(Name = "密碼")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string code { get; set; }
        public string state { get; set; }

        public string stateReturnedByGoogle { get; set; }
        public int? character { get; set; }
    }

    public class AccountOpen
    {
        public string Gu { get; set; }

        public string Ch { get; set; }

        public string il { get; set; }
    }

    public class CurrentUser
    {
        public string Role { get; set; }

        public int UserId { get; set; }

        public string Email { get; set; }
    }

    public class PasswordChange
    {
        public int UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordConfirmation { get; set; }
    }

    public class SearchTags
    {
        public string ChannelTags { get; set; }
        public string SectorTags { get; set; }
        public string Fans { get; set; }
    }

    public class Tokenclass
    {
        public string access_token { get; set; }

        public string token_type { get; set; }

        public int expires_in { get; set; }

        public string refresh_token { get; set; }
    }

    public class Userclass
    {
        public string id { get; set; }

        public string name { get; set; }

        public string given_name { get; set; }

        public string family_name { get; set; }

        public string link { get; set; }

        public string picture { get; set; }

        public string gender { get; set; }

        public string email { get; set; }

        public string locale { get; set; }
    }
}