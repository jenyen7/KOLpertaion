using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KOLperation.Models
{
    public class UserKOL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int KolId { get; set; }

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

        [Display(Name = "暱稱")]
        [StringLength(100)]
        public string Username { get; set; }

        [Display(Name = "頭像")]
        [StringLength(500)]
        public string Avatar { get; set; }

        [Display(Name = "KOL簡介")]
        public string KOLProfile { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "信箱")]
        [StringLength(200)]
        public string Email { get; set; }

        [Display(Name = "電話")]
        [StringLength(50)]
        public string Phone { get; set; }

        [Display(Name = "產業標籤")]
        [StringLength(100)]
        public string SectorTags { get; set; }

        [Display(Name = "註冊時間")]
        public DateTime JoinedDate { get; set; }

        [Display(Name = "帳號存取")]
        public int Enabled { get; set; }

        [Display(Name = "平台連結")]
        public virtual ICollection<UserKOLChannelDetail> ChannelDetails { get; set; }

        [Display(Name = "儲存公司")]
        public virtual ICollection<UserKOLFavoriteCompany> FavoriteCompanies { get; set; }

        [Display(Name = "儲存業配")]
        public virtual ICollection<UserKOLFavoriteSC> FavoriteSCs { get; set; }

        [Display(Name = "合作紀錄")]
        public virtual ICollection<Coop> Coops { get; set; }
    }
}