using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace auth.in2sport.infrastructure.Repositories.Postgres
{
    public class PostgresDbContext: DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(@"Host=localhost;Username=postgres;Password=Admin123;Database=in2sports");
        }

        public DbSet<Users> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Users>().HasKey(u => u.Id);
            modelBuilder.Entity<Users>(ConfigureUser);
        }

        private void ConfigureUser(EntityTypeBuilder<Users> builder)
        {
            builder.ToTable("Users");

            builder.Property(u => u.Email).HasColumnName("email");
            builder.Property(u => u.Password).HasColumnName("password");
            builder.Property(u => u.status).HasColumnName("status");
            builder.Property(u => u.Id).HasColumnName("id");
        }
    }
}
