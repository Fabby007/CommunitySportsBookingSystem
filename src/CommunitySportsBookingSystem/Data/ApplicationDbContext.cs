using CommunitySportsBookingSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CommunitySportsBookingSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sport> Sports => Set<Sport>();
    public DbSet<MemberSport> MemberSports => Set<MemberSport>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<FacilitySport> FacilitySports => Set<FacilitySport>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Sport>(entity =>
        {
            entity.HasIndex(s => s.Name).IsUnique();
        });

        builder.Entity<MemberSport>(entity =>
        {
            entity.HasKey(ms => new { ms.MemberId, ms.SportId });
            entity.HasOne(ms => ms.Member)
                .WithMany(m => m.MemberSports)
                .HasForeignKey(ms => ms.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ms => ms.Sport)
                .WithMany(s => s.MemberSports)
                .HasForeignKey(ms => ms.SportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FacilitySport>(entity =>
        {
            entity.HasKey(fs => new { fs.FacilityId, fs.SportId });
            entity.HasOne(fs => fs.Facility)
                .WithMany(f => f.FacilitySports)
                .HasForeignKey(fs => fs.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(fs => fs.Sport)
                .WithMany(s => s.FacilitySports)
                .HasForeignKey(fs => fs.SportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Facility>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint("CK_Facility_ClosingAfterOpening", "[ClosingTime] > [OpeningTime]"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Facility_Capacity", "[Capacity] IS NULL OR [Capacity] > 0"));
        });

        builder.Entity<Booking>(entity =>
        {
            entity.Property(b => b.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(b => b.Member)
                .WithMany(m => m.Bookings)
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Facility)
                .WithMany(f => f.Bookings)
                .HasForeignKey(b => b.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(t => t.HasCheckConstraint("CK_Booking_EndAfterStart", "[EndTime] > [StartTime]"));

            entity.HasIndex(b => new { b.FacilityId, b.BookingDate });
        });

        builder.Entity<Review>(entity =>
        {
            entity.HasIndex(r => r.BookingId).IsUnique();

            entity.HasOne(r => r.Member)
                .WithMany(m => m.Reviews)
                .HasForeignKey(r => r.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Facility)
                .WithMany(f => f.Reviews)
                .HasForeignKey(r => r.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Booking)
                .WithOne(b => b.Review)
                .HasForeignKey<Review>(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "[Rating] BETWEEN 1 AND 5"));
        });
    }
}
