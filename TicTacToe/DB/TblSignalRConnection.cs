using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TicTacToe.DB
{
    [Table("TblSignalRConnection")]
    public class TblSignalRConnection
    {
        [Key]
        public long ID { get; set; }
        public string? SignalRConnectionID { get; set; }
        public string? UserName { get; set; }
        public string? UserType { get; set; }
        public string? UserID { get; set; }
        public string? brwserInfo { get; set; }

    }
}
