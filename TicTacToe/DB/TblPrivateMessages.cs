using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicTacToe.DB
{
    [Table("TblPrivateMessages")]
    public class TblPrivateMessages
    {
        [Key]
        public long PrivateMessageId { get; set; }
        public long PrivateChatId { get; set; }
        public long SenderUserId { get; set; }
        public long ReceiverUserId { get; set; }
        public string? MessageText { get; set; }
        public bool? IsSeen { get; set; }
        public bool? IsDeleted { get; set; } 
        public DateTime? Datetime { get; set; }
    }
}
