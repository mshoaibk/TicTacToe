using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TicTacToe.DB
{
    [Table("Tbl_User")]
    public class Tbl_User
    {
        [Key]
        public long Id { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsActive { get; set; }
        public string? CreateBY { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
