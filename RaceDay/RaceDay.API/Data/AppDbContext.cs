using RaceDay.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Database tables
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RaceDay.Api.Models.Route> Routes { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // -------------------------
            // USERS
            // -------------------------

            // Email must be unique.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Allowed user roles.
            modelBuilder.Entity<User>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Users_Role",
                    "[Role] IN ('Organiser', 'Participant')"));

            // Default date when a user is created.
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");


            // -------------------------
            // EVENTS
            // -------------------------

            // An event belongs to one organiser.
            // An organiser can have many events.
            modelBuilder.Entity<Event>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Allowed event statuses.
            modelBuilder.Entity<Event>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Events_Status",
                    "[Status] IN ('Draft', 'Published', 'Cancelled', 'Completed')"));

            // Default event status.
            modelBuilder.Entity<Event>()
                .Property(e => e.Status)
                .HasDefaultValue("Draft");

            // Default date when an event is created.
            modelBuilder.Entity<Event>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETDATE()");


            // -------------------------
            // CATEGORIES
            // -------------------------

            // A category belongs to one event.
            // An event can have many categories.
            modelBuilder.Entity<Category>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Match SQL DECIMAL(6,2).
            modelBuilder.Entity<Category>()
                .Property(c => c.DistanceKm)
                .HasPrecision(6, 2);

            // Match SQL DECIMAL(8,2).
            modelBuilder.Entity<Category>()
                .Property(c => c.EntryFee)
                .HasPrecision(8, 2)
                .HasDefaultValue(0);


            // -------------------------
            // ROUTES
            // -------------------------

            // A category can have one route.
            // CategoryId must therefore be unique.
            modelBuilder.Entity<RaceDay.Api.Models.Route>()
                .HasIndex(r => r.CategoryId)
                .IsUnique();

            // A route belongs to one category.
            modelBuilder.Entity<RaceDay.Api.Models.Route>()
                .HasOne<Category>()
                .WithOne()
                .HasForeignKey<RaceDay.Api.Models.Route>(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Match SQL DECIMAL(6,2).
            modelBuilder.Entity<RaceDay.Api.Models.Route>()
                .Property(r => r.DistanceKm)
                .HasPrecision(6, 2);


            // -------------------------
            // ENROLMENTS
            // -------------------------

            // An enrolment belongs to one category.
            modelBuilder.Entity<Enrolment>()
                .HasOne<Category>()
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // An enrolment belongs to one participant.
            modelBuilder.Entity<Enrolment>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            // A participant cannot enrol in the same category twice.
            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.CategoryId, e.ParticipantId })
                .IsUnique();

            // Allowed enrolment statuses.
            modelBuilder.Entity<Enrolment>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Enrolments_Status",
                    "[Status] IN ('Pending', 'Confirmed', 'Cancelled')"));

            // Allowed payment statuses.
            modelBuilder.Entity<Enrolment>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Enrolments_PaymentStatus",
                    "[PaymentStatus] IN ('Unpaid', 'Paid', 'Refunded')"));

            // Default enrolment status.
            modelBuilder.Entity<Enrolment>()
                .Property(e => e.Status)
                .HasDefaultValue("Pending");

            // Default payment status.
            modelBuilder.Entity<Enrolment>()
                .Property(e => e.PaymentStatus)
                .HasDefaultValue("Unpaid");

            // Default enrolment date.
            modelBuilder.Entity<Enrolment>()
                .Property(e => e.EnrolmentDate)
                .HasDefaultValueSql("GETDATE()");


            // -------------------------
            // RESULTS
            // -------------------------

            // Each enrolment can only have one result.
            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentId)
                .IsUnique();

            // A result belongs to one enrolment.
            modelBuilder.Entity<Result>()
                .HasOne<Enrolment>()
                .WithOne()
                .HasForeignKey<Result>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Allowed result statuses.
            modelBuilder.Entity<Result>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Results_Status",
                    "[Status] IN ('Finished', 'DNF', 'DNS')"));

            // Default result status.
            modelBuilder.Entity<Result>()
                .Property(r => r.Status)
                .HasDefaultValue("Finished");

            // Default recorded date.
            modelBuilder.Entity<Result>()
                .Property(r => r.RecordedAt)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}