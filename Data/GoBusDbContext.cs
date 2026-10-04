using GoBus.Models;
using Microsoft.EntityFrameworkCore;

namespace GoBus.Data;

public class GoBusDbContext : DbContext
{
    public GoBusDbContext(DbContextOptions<GoBusDbContext> options)
        : base(options)
    {
    }


    // ==========================================
    // DATABASE TABLES
    // ==========================================

    public DbSet<BusCompany> BusCompanies { get; set; }

    public DbSet<Bus> Buses { get; set; }

    public DbSet<GoBus.Models.Route> Routes { get; set; }

    public DbSet<Trip> Trips { get; set; }

    public DbSet<Seat> Seats { get; set; }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingDetail> BookingDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // ==========================================
        // TRIP - PRICE
        // ==========================================

        modelBuilder.Entity<Trip>()
            .Property(t => t.Price)
            .HasPrecision(18, 2);


        // ==========================================
        // BUS COMPANY - BUS
        // ==========================================

        modelBuilder.Entity<Bus>()
            .HasOne(b => b.BusCompany)
            .WithMany(c => c.Buses)
            .HasForeignKey(b => b.BusCompanyId)
            .OnDelete(DeleteBehavior.Cascade);


        // ==========================================
        // BUS COMPANY - TRIP
        // ==========================================

        modelBuilder.Entity<Trip>()
            .HasOne(t => t.BusCompany)
            .WithMany(c => c.Trips)
            .HasForeignKey(t => t.BusCompanyId)
            .OnDelete(DeleteBehavior.NoAction);


        // ==========================================
        // BUS - TRIP
        // ==========================================

        modelBuilder.Entity<Trip>()
            .HasOne(t => t.Bus)
            .WithMany(b => b.Trips)
            .HasForeignKey(t => t.BusId)
            .OnDelete(DeleteBehavior.NoAction);


        // ==========================================
        // ROUTE - TRIP
        // ==========================================

        modelBuilder.Entity<Trip>()
            .HasOne(t => t.Route)
            .WithMany(r => r.Trips)
            .HasForeignKey(t => t.RouteId)
            .OnDelete(DeleteBehavior.NoAction);


        // ==========================================
        // TRIP - SEAT
        // ==========================================

        modelBuilder.Entity<Seat>()
            .HasOne(s => s.Trip)
            .WithMany(t => t.Seats)
            .HasForeignKey(s => s.TripId)
            .OnDelete(DeleteBehavior.Cascade);


        // ==========================================
        // SEAT NUMBER
        // ==========================================

        modelBuilder.Entity<Seat>()
            .Property(s => s.SeatNumber)
            .HasMaxLength(20)
            .IsRequired();


        // ==========================================
        // SEAT TYPE
        // ==========================================

        modelBuilder.Entity<Seat>()
            .Property(s => s.SeatType)
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.Entity<Booking>()
            .Property(b => b.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<BookingDetail>()
            .Property(b => b.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Customer)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Trip)
            .WithMany()
            .HasForeignKey(b => b.TripId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BookingDetail>()
            .HasOne(bd => bd.Booking)
            .WithMany(b => b.BookingDetails)
            .HasForeignKey(bd => bd.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookingDetail>()
            .HasOne(bd => bd.Seat)
            .WithMany()
            .HasForeignKey(bd => bd.SeatId)
            .OnDelete(DeleteBehavior.NoAction);
        
        modelBuilder.Entity<User>()
            .HasOne(u => u.Customer)
            .WithOne()
            .HasForeignKey<User>(u => u.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}