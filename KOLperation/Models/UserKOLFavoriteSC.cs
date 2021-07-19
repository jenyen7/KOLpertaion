using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KOLperation.Models
{
    public class UserKOLFavoriteSC
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int KOLId { get; set; }

        [ForeignKey("KOLId")]
        public virtual UserKOL UserKOL { get; set; }

        [Display(Name = "KOL儲存的業配")]
        public int SponsoredContentId { get; set; }

        [ForeignKey("SponsoredContentId")]
        public virtual SponsoredContent SponsoredContent { get; set; }

        public DateTime Record { get; set; }
    }
}