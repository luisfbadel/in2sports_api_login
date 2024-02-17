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

            builder.Property(u => u.Id).HasColumnName("id");
            builder.Property(u => u.Email).HasColumnName("email");
            builder.Property(u => u.Password).HasColumnName("password");
            builder.Property(u => u.Status).HasColumnName("status");
            builder.Property(u => u.TypeUser).HasColumnName("type_user");
            builder.Property(u => u.FirstName).HasColumnName("first_name");
            builder.Property(u => u.SecondName).HasColumnName("second_name");
            builder.Property(u => u.FirstLastname).HasColumnName("first_lastname");
            builder.Property(u => u.SecondLastname).HasColumnName("second_lastname");
            builder.Property(u => u.TypeDocument).HasColumnName("type_document");
            builder.Property(u => u.DocumentNumber).HasColumnName("document_number");
            builder.Property(u => u.PhoneNumber).HasColumnName("phone_number");
            builder.Property(u => u.Address).HasColumnName("address");
        }
    }
}
