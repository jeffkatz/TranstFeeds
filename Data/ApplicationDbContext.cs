using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TransitFeeds.Models;

namespace TransitFeeds.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // GTFS tables
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<TransitCalendar> TransitCalendars { get; set; }
        public DbSet<Stop> Stops { get; set; }
        public DbSet<ShapesMaster> ShapesMasters { get; set; }
        public DbSet<Shape> Shapes { get; set; }
        public DbSet<TransitRoute> TransitRoutes { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<StopTime> StopTimes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // TimeSpan converter for StopTimes
            // =========================
            var timeConverter = new ValueConverter<TimeSpan?, string>(
                v => v.HasValue ? v.Value.ToString(@"hh\:mm\:ss") : null!,
                v => string.IsNullOrEmpty(v) ? (TimeSpan?)null : TimeSpan.Parse(v)
            );

            // =========================
            // StopTimes
            // =========================
            modelBuilder.Entity<StopTime>(eb =>
            {
                eb.HasKey(st => st.Id); // Internal PK

                eb.Property(st => st.ArrivalTime)
                  .HasColumnName("arrival_time")
                  .HasColumnType("varchar(8)")
                  .HasConversion(timeConverter);

                eb.Property(st => st.DepartureTime)
                  .HasColumnName("departure_time")
                  .HasColumnType("varchar(8)")
                  .HasConversion(timeConverter);

                eb.HasOne(st => st.Trip)
                  .WithMany(t => t.StopTimes)
                  .HasForeignKey(st => st.TripId)
                  .OnDelete(DeleteBehavior.Cascade);

                eb.HasOne(st => st.Stop)
                  .WithMany(s => s.StopTimes)
                  .HasForeignKey(st => st.StopId)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // Agencies
            // =========================
            modelBuilder.Entity<Agency>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<Agency>()
                .HasMany(a => a.TransitRoutes)
                .WithOne(r => r.Agency)
                .HasForeignKey(r => r.AgencyId);

            // =========================
            // TransitCalendars
            // =========================
            modelBuilder.Entity<TransitCalendar>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<TransitCalendar>()
                .HasMany(c => c.Trips)
                .WithOne(t => t.TransitCalendar)
                .HasForeignKey(t => t.ServiceId);

            // =========================
            // Stops
            // =========================
            modelBuilder.Entity<Stop>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Stop>()
                .Property(s => s.StopLat)
                .HasColumnType("decimal(9,6)");

            modelBuilder.Entity<Stop>()
                .Property(s => s.StopLon)
                .HasColumnType("decimal(9,6)");

            modelBuilder.Entity<Stop>()
                .HasOne(s => s.ParentStation)
                .WithMany()
                .HasForeignKey(s => s.ParentStationId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // ShapesMaster
            // =========================
            modelBuilder.Entity<ShapesMaster>()
                .HasKey(sm => sm.Id);

            modelBuilder.Entity<ShapesMaster>()
                .HasMany(sm => sm.Shapes)
                .WithOne(s => s.ShapesMaster)
                .HasForeignKey(s => s.ShapeId);

            modelBuilder.Entity<ShapesMaster>()
                .HasMany(sm => sm.Trips)
                .WithOne(t => t.ShapesMaster)
                .HasForeignKey(t => t.ShapeId);

            // =========================
            // Shapes (Points)
            // =========================
            modelBuilder.Entity<Shape>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Shape>()
                .Property(s => s.ShapePtLat)
                .HasColumnType("decimal(9,6)");

            modelBuilder.Entity<Shape>()
                .Property(s => s.ShapePtLon)
                .HasColumnType("decimal(9,6)");

            modelBuilder.Entity<Shape>()
                .Property(s => s.ShapeDistTraveled)
                .HasColumnType("decimal(18,2)");

            // =========================
            // TransitRoutes
            // =========================
            modelBuilder.Entity<TransitRoute>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<TransitRoute>()
                .HasMany(r => r.Trips)
                .WithOne(t => t.TransitRoute)
                .HasForeignKey(t => t.TransitRouteId);

            // =========================
            // Trips
            // =========================
            modelBuilder.Entity<Trip>()
                .HasKey(t => t.Id);
        }
    }
}
