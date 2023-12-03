using System.ComponentModel.DataAnnotations.Schema;

namespace TicTacToe.DB
{
    [Table("TblGameBoard")]
    public class TblGameBoard
    {
        public long Id { get; set; }
        public long FromUserId { get; set; }
        public long ToUserId { get; set; }
        public bool? IsAccepted { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long? WinnerUserId { get; set; }
    }
}
