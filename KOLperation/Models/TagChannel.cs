using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KOLperation.Models
{
    public class TagChannel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChannelId { get; set; }

        [Display(Name = "標籤編號")]
        [StringLength(50)]
        public string TagId { get; set; }

        [Display(Name = "平台標籤名稱")]
        [StringLength(50)]
        public string TagName { get; set; }

        [Display(Name = "圖標")]
        [StringLength(50)]
        public string TagIcon { get; set; }

        [Display(Name = "Font Awesome Id")]
        [StringLength(50)]
        public string FAid { get; set; }
    }
}