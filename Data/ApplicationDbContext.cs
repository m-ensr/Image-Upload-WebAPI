using Microsoft.EntityFrameworkCore;
using image_upload.Models;

namespace image_upload.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ImageCollection> ImageCollections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ImageCollection>()
                .HasKey(i => i.UserID);
        }
    }
}
