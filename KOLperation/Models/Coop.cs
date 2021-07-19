using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KOLperation.Models
{
    public class Coop
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CoopId { get; set; }

        public int Status { get; set; }

        public int? MessageHistoryId { get; set; }

        public int KOLId { get; set; }

        [ForeignKey("KOLId")]
        public virtual UserKOL UserKOL { get; set; }

        public int SponsoredContentId { get; set; }

        [ForeignKey("SponsoredContentId")]
        public virtual SponsoredContent SponsoredContent { get; set; }

        public DateTime? CoopSuccessDate { get; set; }
    }
}