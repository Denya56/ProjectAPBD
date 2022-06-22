using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.Models.UserAuth;

namespace Project.Context
{
    public class ProjectContext : DbContext
    {
        private readonly IConfiguration _configuration;
        public ProjectContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ProjectContext(DbContextOptions<ProjectContext> options) : base (options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("Project"));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Watchlist>()
                .HasKey(w => new { w.IdUser, w.IdCompany });
            modelBuilder.Entity<Watchlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Watchlists)
                .HasForeignKey(w => w.IdUser);
            modelBuilder.Entity<Watchlist>()
                .HasOne(w => w.Company)
                .WithMany(c => c.Watchlists)
                .HasForeignKey(w => w.IdCompany);
        }
        public DbSet<Company> Companies { get; set; }
        /*public DbSet<PriceDaily> PricesDaily { get; set; }
        public DbSet<PricesTimeSpan> PricesTimeSpans { get; set; }
        public DbSet<Result> Results { get; set; }*/
        public DbSet<User> Users { get; set; }
        public DbSet<PricesTimeSpan> PricesTimeSpans { get; set; }
        public DbSet<Watchlist> Watchlists { get; set; }
    }
}
