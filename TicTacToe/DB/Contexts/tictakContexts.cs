using Microsoft.EntityFrameworkCore;

namespace TicTacToe.DB.Contexts
{
    public class tictakContexts: DbContext
    {
        private readonly DbContextOptions _options;

        public tictakContexts(DbContextOptions<tictakContexts> options) : base(options)
        {
            _options = options;
        }
        protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tbl_User>().Property(e => e.Id).IsRequired().UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<TblSignalRConnection>().Property(e => e.ID).IsRequired().UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<TblPrivateChats>().Property(e => e.PrivateChatId).IsRequired().UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<TblPrivateMessages>().Property(e => e.PrivateMessageId).IsRequired().UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<TblGameBoard>().Property(e => e.Id).IsRequired().UsePropertyAccessMode(PropertyAccessMode.Field);

        }
        public DbSet<Tbl_User> Tbl_User { get; set; }
        public DbSet<TblSignalRConnection> TblSignalRConnection { get; set; }
        public DbSet<TblPrivateChats> TblPrivateChats { get; set; }
        public DbSet<TblPrivateMessages> TblPrivateMessages { get; set; }
        public DbSet<TblGameBoard> TblGameBoard { get; set; }

    }
}
