using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KOLperation.Models
{
    public class UserKOLFavoriteCompany
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int KOLId { get; set; }

        [ForeignKey("KOLId")]
        public virtual UserKOL UserKOL { get; set; }

        [Display(Name = "KOL儲存的公司")]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual UserCompany UserCompany { get; set; }

        public DateTime Record { get; set; }
    }
}