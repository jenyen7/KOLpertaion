using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KOLperation.Models
{
    public class MessageHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MsgId { get; set; }

        [Display(Name = "KolId")]
        public int KolId { get; set; }

        [ForeignKey("KolId")]
        public UserKOL UserKOL { get; set; }

        [Display(Name = "業配號碼")]
        public int SponsoredContentId { get; set; }

        [ForeignKey("SponsoredContentId ")]
        public SponsoredContent SponsoredContent { get; set; }

        public virtual ICollection<MessageHistoryContent> MessageHistoryContents { get; set; }
    }

    public class MessageHistoryContent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MsgContentId { get; set; }

        [Display(Name = "訊息Id")]
        public int MsgId { get; set; }

        [ForeignKey("MsgId")]
        public MessageHistory MessageHistory { get; set; }

        [Display(Name = "誰發的訊息")]
        public int Sender { get; set; }

        [Display(Name = "訊息")]
        public string Message { get; set; }

        [Display(Name = "傳送時間")]
        public DateTime MessageTime { get; set; }
    }
}