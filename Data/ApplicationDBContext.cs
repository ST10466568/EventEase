
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VenueBooking.Models; // Ensure your models are correctly referenced here

namespace VenueBooking.Data
{
    public class ApplicationUser : IdentityUser
    {
        // You can add additional properties here if needed
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<EventType> EventTypes { get; set; }

        public override int SaveChanges()
        {
            SetAuditProperties();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditProperties();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetAuditProperties()
        {
            var entriesEvent = ChangeTracker.Entries().Where(e => e.Entity is Event && (e.State == EntityState.Added));
            var entriesVenue = ChangeTracker.Entries().Where(e => e.Entity is Venue && (e.State == EntityState.Added));
            var entriesBooking = ChangeTracker.Entries().Where(e => e.Entity is Booking && (e.State == EntityState.Added));

            foreach (var entry in entriesEvent)
            {
                if (entry.Entity is Event eventItem)
                {
                    if (entry.State == EntityState.Added)
                    {
                        eventItem.CreatedDate = DateTime.Now;
                        eventItem.CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
                    }
                }              
            }

            foreach (var entry in entriesVenue)
            {
                if (entry.Entity is Venue venue)
                {
                    if (entry.State == EntityState.Added)
                    {
                        venue.CreatedDate = DateTime.Now;
                        venue.CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
                    }
                }
            }

            foreach (var entry in entriesBooking)
            {
                if (entry.Entity is Booking booking)
                {
                    if (entry.State == EntityState.Added)
                    {
                        booking.CreatedDate = DateTime.Now;
                        booking.CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
                    }
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany()
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Venue)
                .WithMany()
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.NoAction);

                modelBuilder.Entity<Booking>()
        .HasOne(b => b.Event)
        .WithMany(e => e.Bookings)
        .HasForeignKey(b => b.EventId)
        .OnDelete(DeleteBehavior.Cascade); // or .Restrict if needed
        }
    }

    
}
