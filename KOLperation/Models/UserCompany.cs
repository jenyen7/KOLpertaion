using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KOLperation.Models
{
    public class UserCompany
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ComId { get; set; }

        [Display(Name = "亂碼ID")]
        [StringLength(200)]
        public string Guid { get; set; }

        [Display(Name = "帳號")]
        [StringLength(100)]
        public string AccountId { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; }

        [Display(Name = "密碼鹽")]
        public string PasswordSalt { get; set; }

        [Display(Name = "統一編號")]
        [StringLength(50)]
        public string TaxIdNumber { get; set; }

        [Display(Name = "公司名稱")]
        [StringLength(100)]
        public string Company { get; set; }

        [Display(Name = "公司Logo")]
        [StringLength(500)]
        public string CompanyLogo { get; set; }

        [Display(Name = "公司簡介")]
        public string CompanyProfile { get; set; }

        [Display(Name = "公司網站")]
        [StringLength(100)]
        public string Website { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "信箱")]
        [StringLength(100)]
        public string Email { get; set; }

        [Display(Name = "電話")]
        [StringLength(50)]
        public string Phone { get; set; }

        [Display(Name = "手機")]
        [StringLength(50)]
        public string Cellphone { get; set; }

        [Display(Name = "地址")]
        [StringLength(200)]
        public string Address { get; set; }

        [Display(Name = "負責人")]
        [StringLength(50)]
        public string PersonInCharge { get; set; }

        [Display(Name = "平台標籤")]
        [StringLength(100)]
        public string ChannelTags { get; set; }

        [Display(Name = "產業標籤")]
        [StringLength(100)]
        public string SectorTags { get; set; }

        [Display(Name = "註冊時間")]
        public DateTime JoinedDate { get; set; }

        [Display(Name = "帳號存取")]
        public int Enabled { get; set; }

        [Display(Name = "儲存KOL")]
        public virtual ICollection<UserCompanyFavoriteKOL> FavoriteKOLs { get; set; }

        [Display(Name = "業配列表")]
        public virtual ICollection<SponsoredContent> SponsoredContents { get; set; }
    }
}