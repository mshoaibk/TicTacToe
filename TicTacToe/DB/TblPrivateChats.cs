using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicTacToe.DB
{
    [Table("TblPrivateChats")]
    public class TblPrivateChats
    {
        [Key]
        public long PrivateChatId { get; set; }
        public long Sender_UserId { get; set; } 
        public long ReciverId_UserId { get; set; } 
        public string? SenderName { get; set; } 
        public string? ReciverName { get; set; } 
        public string? LastMessageBody { get; set; }
        public DateTime? LastDateTime { get; set; }
    }
}
