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
        public DbSet<UserType> TypeUser { get; set; }
        public DbSet<UserSubscription> UserSubscription { get; set; }
        public DbSet<AgeRange> AgeRange { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Users>().HasKey(u => u.Id);
            modelBuilder.Entity<Users>(ConfigureUser);

            modelBuilder.Entity<UserType>().HasKey(u => u.Id);
            modelBuilder.Entity<UserType>(ConfigureUserType);

            modelBuilder.Entity<UserSubscription>().HasKey(u => u.Id);
            modelBuilder.Entity<UserSubscription>(ConfigureUserSubscription);

            modelBuilder.Entity<AgeRange>().HasKey(u => u.Id);
            modelBuilder.Entity<AgeRange>(ConfigureAgeRange);
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
        }

        private void ConfigureUserType(EntityTypeBuilder<UserType> builder)
        {
            builder.ToTable("user_type");

            builder.Property(u => u.Id).HasColumnName("id");
            builder.Property(u => u.DescriptionType).HasColumnName("description_type");
            builder.Property(u => u.Status).HasColumnName("status");
        }

        private void ConfigureUserSubscription(EntityTypeBuilder<UserSubscription> builder)
        {
            builder.ToTable("user_subscription");

            builder.Property(u => u.Id).HasColumnName("id");
            builder.Property(u => u.UserId).HasColumnName("user_id");
            builder.Property(u => u.CourseId).HasColumnName("course_id");
            builder.Property(u => u.MonthsSubscribed).HasColumnName("months_subscribed");
            builder.Property(u => u.LastDate).HasColumnName("last_date");

        }

        private void ConfigureAgeRange(EntityTypeBuilder<AgeRange> builder)
        {
            builder.ToTable("age_range");

            builder.Property(u => u.Id).HasColumnName("id");
            builder.Property(u => u.StartAge).HasColumnName("start_age");
            builder.Property(u => u.EndAge).HasColumnName("end_age");

        }
    }
}
