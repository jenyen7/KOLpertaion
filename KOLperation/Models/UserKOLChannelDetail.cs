using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KOLperation.Models
{
    public class UserKOLChannelDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DetailId { get; set; }

        [Display(Name = "KOL ID")]
        public int KolId { get; set; }

        [ForeignKey("KolId")]
        public UserKOL UserKOL { get; set; }

        [Display(Name = "平台類型ID")]
        public int ChannelId { get; set; }

        [ForeignKey("ChannelId")]
        [StringLength(50)]
        public TagChannel TagChannel { get; set; }

        [Display(Name = "連結")]
        [StringLength(100)]
        public string Url { get; set; }

        [Display(Name = "粉絲人數")]
        public int? FansNumber { get; set; }
    }
}