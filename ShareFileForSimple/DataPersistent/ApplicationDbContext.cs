using Microsoft.EntityFrameworkCore;
using ShareFileForSimple.DataPersistent.Model;

namespace ShareFileForSimple.DataPersistent
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<FileItemModel> FileItemModels { set; get; }
        public DbSet<FileRecordModel> FileRecordModels { set; get; }
        public ApplicationDbContext()
        {
        }
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
