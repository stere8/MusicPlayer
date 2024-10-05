using Microsoft.EntityFramweworks;

namespace SonorousAPI.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Song> Songs { get; set; }
    }
}