using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KOLperation.Models
{
    public class SponsoredContent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScId { get; set; }

        [Display(Name = "案件名稱")]
        [StringLength(200)]
        public string Title { get; set; }

        [Display(Name = "合作預算")]
        [StringLength(100)]
        public string Budget { get; set; }

        [Display(Name = "需求人數")]
        public int? PeopleRequired { get; set; }

        [Display(Name = "需求條件")]
        [StringLength(100)]
        public string MinimumRequirement { get; set; }

        [Display(Name = "合作內容")]
        public string Detail { get; set; }

        [Display(Name = "產品照片")]
        [StringLength(500)]
        public string ProductPicture { get; set; }

        [Display(Name = "負責人")]
        [StringLength(50)]
        public string PersonInCharge { get; set; }

        [Display(Name = "粉絲最低需求人數")]
        [StringLength(50)]
        public string FansNumberMinimum { get; set; }

        [Display(Name = "平台標籤")]
        [StringLength(100)]
        public string ChannelTags { get; set; }

        [Display(Name = "產業標籤")]
        [StringLength(100)]
        public string SectorTags { get; set; }

        [Display(Name = "截止日期")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "開啟or關閉")]
        public int SCstatus { get; set; }

        [Display(Name = "合作紀錄")]
        public virtual ICollection<Coop> Coops { get; set; }

        [Display(Name = "是哪間公司的")]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual UserCompany UserCompany { get; set; }
    }
}