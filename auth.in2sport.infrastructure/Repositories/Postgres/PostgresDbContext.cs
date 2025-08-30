using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;

namespace auth.in2sport.infrastructure.Repositories.Postgres
{
    public class PostgresDbContext: DbContext
    {
        private readonly IConfiguration _config;

        public PostgresDbContext(IConfiguration config)
        {
            _config = config;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_config.GetConnectionString("DefaultConnection"));
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<UserType> TypeUser { get; set; }
        public DbSet<AgeRange> AgeRange { get; set; }
        public DbSet<RefreshTokenHistory> RefreshTokenHistory { get; set; }
        public DbSet<RecoverPassword> RecoverPassword {  get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Users>().HasKey(u => u.Id);
            modelBuilder.Entity<Users>(ConfigureUser);

            modelBuilder.Entity<UserType>().HasKey(u => u.Id);
            modelBuilder.Entity<UserType>(ConfigureUserType);

            modelBuilder.Entity<AgeRange>().HasKey(u => u.Id);
            modelBuilder.Entity<AgeRange>(ConfigureAgeRange);

            modelBuilder.Entity<RefreshTokenHistory>().HasKey(u => u.Id);
            modelBuilder.Entity<RefreshTokenHistory>(ConfigureRefreshTokenHistory);

            modelBuilder.Entity<RecoverPassword>().HasKey(u => u.Id);
            modelBuilder.Entity<RecoverPassword>(ConfigureRecoverPassword);
        }

        private void ConfigureUser(EntityTypeBuilder<Users> builder)
        {
            builder.ToTable("users");

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
            builder.Property(u => u.CreationDate).HasColumnName("creation_date");
            builder.Property(u => u.PasswordValidation).HasColumnName("password_validation");
            builder.Property(u => u.Birthdate).HasColumnName("birthdate");
            builder.Property(u => u.InstitutionName).HasColumnName("institution_name");
            builder.Property(u => u.EmailValidation).HasColumnName("email_validation");
            builder.Property(u => u.TokenConfirmation).HasColumnName("token_confirmation");
            builder.Property(u => u.Departament).HasColumnName("departament");
            builder.Property(u => u.City).HasColumnName("city");
            builder.Property(u => u.AcceptedConditions).HasColumnName("accepted_conditions");
        }

        private void ConfigureUserType(EntityTypeBuilder<UserType> builder)
        {
            builder.ToTable("user_type");

            builder.Property(u => u.Id).HasColumnName("id");
            builder.Property(u => u.DescriptionType).HasColumnName("description_type");
            builder.Property(u => u.Status).HasColumnName("status");
        }

        private void ConfigureAgeRange(EntityTypeBuilder<AgeRange> builder)
        {
            builder.ToTable("age_range");

            builder.Property(u => u.Id).HasColumnName("id");
            builder.Property(u => u.StartAge).HasColumnName("start_age");
            builder.Property(u => u.EndAge).HasColumnName("end_age");
            builder.Property(u => u.Description).HasColumnName("description");
        }

        private void ConfigureRefreshTokenHistory(EntityTypeBuilder<RefreshTokenHistory> builder)
        {
            builder.ToTable("refresh_token_history");

            builder.Property(u => u.Id).HasColumnName("id");
            builder.Property(u => u.UserId).HasColumnName("user_id");
            builder.Property(u => u.Token).HasColumnName("token");
            builder.Property(u => u.RefreshToken).HasColumnName("refresh_token");
            builder.Property(u => u.CreationDate).HasColumnName("creation_date").HasColumnType("timestamp with time zone");
            builder.Property(u => u.ExpirationDate).HasColumnName("expiration_date").HasColumnType("timestamp with time zone");
        }

        private void ConfigureRecoverPassword(EntityTypeBuilder<RecoverPassword> builder)
        {
            builder.ToTable("recover_password");

            builder.Property(u => u.Id).HasColumnName("id");
            builder.Property(u => u.UserId).HasColumnName("user_id");
            builder.Property(u => u.Email).HasColumnName("email");
            builder.Property(u => u.RecoverTime).HasColumnName("recover_time").HasColumnType("timestamp with time zone");
            builder.Property(u => u.Code).HasColumnName("code");
        }
    }
}
