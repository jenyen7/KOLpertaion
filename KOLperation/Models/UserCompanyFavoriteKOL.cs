using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KOLperation.Models
{
    public class UserCompanyFavoriteKOL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual UserCompany UserCompany { get; set; }

        [Display(Name = "公司儲存的KOL")]
        public int KOLId { get; set; }

        [ForeignKey("KOLId")]
        public virtual UserKOL UserKOL { get; set; }

        public DateTime Record { get; set; }
    }
}