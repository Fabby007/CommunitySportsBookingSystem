using CommunitySportsBookingSystem.Data;
using CommunitySportsBookingSystem.Models;
using CommunitySportsBookingSystem.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CommunitySportsBookingSystem.Tests.Services;

public class BookingAvailabilityServiceTests
{
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Facility ActiveFacility(int id = 1, string opening = "08:00", string closing = "20:00") => new()
    {
        FacilityId = id,
        Name = "Test Court",
        FacilityType = "Tennis Court",
        Location = "Test Park",
        IsActive = true,
        OpeningTime = TimeSpan.Parse(opening),
        ClosingTime = TimeSpan.Parse(closing)
    };

    [Theory]
    [InlineData("10:00", "11:00", "10:30", "11:30", true)]  // partial overlap
    [InlineData("10:00", "12:00", "10:30", "11:30", true)]  // full containment
    [InlineData("10:00", "11:00", "11:00", "12:00", false)] // boundary-touch: NOT a conflict
    [InlineData("10:00", "11:00", "09:00", "10:00", false)] // boundary-touch other side
    [InlineData("10:00", "11:00", "12:00", "13:00", false)] // no overlap
    public void RangesOverlap_DetectsOverlapCorrectly(string aStart, string aEnd, string bStart, string bEnd, bool expectedOverlap)
    {
        var service = new BookingAvailabilityService(CreateContext(Guid.NewGuid().ToString()));

        var result = service.RangesOverlap(TimeSpan.Parse(aStart), TimeSpan.Parse(aEnd), TimeSpan.Parse(bStart), TimeSpan.Parse(bEnd));

        Assert.Equal(expectedOverlap, result);
    }

    [Fact]
    public async Task ValidateAsync_RejectsOverlappingActiveBooking_SameFacilitySameDate()
    {
        await using var db = CreateContext(Guid.NewGuid().ToString());
        var facility = ActiveFacility();
        db.Facilities.Add(facility);
        db.Bookings.Add(new Booking
        {
            FacilityId = facility.FacilityId,
            MemberId = "member-1",
            BookingDate = new DateOnly(2026, 9, 10),
            StartTime = TimeSpan.Parse("10:00"),
            EndTime = TimeSpan.Parse("11:00"),
            Status = BookingStatus.Confirmed
        });
        await db.SaveChangesAsync();

        var service = new BookingAvailabilityService(db);
        var errors = await service.ValidateAsync(facility.FacilityId, new DateOnly(2026, 9, 10), TimeSpan.Parse("10:30"), TimeSpan.Parse("11:30"));

        Assert.Contains(errors, e => e.Contains("overlaps"));
    }

    [Fact]
    public async Task ValidateAsync_AllowsBoundaryTouchingBooking()
    {
        await using var db = CreateContext(Guid.NewGuid().ToString());
        var facility = ActiveFacility();
        db.Facilities.Add(facility);
        db.Bookings.Add(new Booking
        {
            FacilityId = facility.FacilityId,
            MemberId = "member-1",
            BookingDate = new DateOnly(2026, 9, 10),
            StartTime = TimeSpan.Parse("10:00"),
            EndTime = TimeSpan.Parse("11:00"),
            Status = BookingStatus.Confirmed
        });
        await db.SaveChangesAsync();

        var service = new BookingAvailabilityService(db);
        // New booking starts exactly when the existing one ends — must be allowed.
        var errors = await service.ValidateAsync(facility.FacilityId, new DateOnly(2026, 9, 10), TimeSpan.Parse("11:00"), TimeSpan.Parse("12:00"));

        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateAsync_IgnoresConflictsOnDifferentFacility()
    {
        await using var db = CreateContext(Guid.NewGuid().ToString());
        var facilityA = ActiveFacility(1);
        var facilityB = ActiveFacility(2);
        db.Facilities.AddRange(facilityA, facilityB);
        db.Bookings.Add(new Booking
        {
            FacilityId = facilityA.FacilityId,
            MemberId = "member-1",
            BookingDate = new DateOnly(2026, 9, 10),
            StartTime = TimeSpan.Parse("10:00"),
            EndTime = TimeSpan.Parse("11:00"),
            Status = BookingStatus.Confirmed
        });
        await db.SaveChangesAsync();

        var service = new BookingAvailabilityService(db);
        var errors = await service.ValidateAsync(facilityB.FacilityId, new DateOnly(2026, 9, 10), TimeSpan.Parse("10:00"), TimeSpan.Parse("11:00"));

        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateAsync_IgnoresConflictsOnDifferentDate()
    {
        await using var db = CreateContext(Guid.NewGuid().ToString());
        var facility = ActiveFacility();
        db.Facilities.Add(facility);
        db.Bookings.Add(new Booking
        {
            FacilityId = facility.FacilityId,
            MemberId = "member-1",
            BookingDate = new DateOnly(2026, 9, 10),
            StartTime = TimeSpan.Parse("10:00"),
            EndTime = TimeSpan.Parse("11:00"),
            Status = BookingStatus.Confirmed
        });
        await db.SaveChangesAsync();

        var service = new BookingAvailabilityService(db);
        var errors = await service.ValidateAsync(facility.FacilityId, new DateOnly(2026, 9, 11), TimeSpan.Parse("10:00"), TimeSpan.Parse("11:00"));

        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateAsync_IgnoresCancelledBookingsWhenCheckingConflicts()
    {
        await using var db = CreateContext(Guid.NewGuid().ToString());
        var facility = ActiveFacility();
        db.Facilities.Add(facility);
        db.Bookings.Add(new Booking
        {
            FacilityId = facility.FacilityId,
            MemberId = "member-1",
            BookingDate = new DateOnly(2026, 9, 10),
            StartTime = TimeSpan.Parse("10:00"),
            EndTime = TimeSpan.Parse("11:00"),
            Status = BookingStatus.Cancelled
        });
        await db.SaveChangesAsync();

        var service = new BookingAvailabilityService(db);
        var errors = await service.ValidateAsync(facility.FacilityId, new DateOnly(2026, 9, 10), TimeSpan.Parse("10:00"), TimeSpan.Parse("11:00"));

        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateAsync_RejectsTimeOutsideOperatingHours()
    {
        await using var db = CreateContext(Guid.NewGuid().ToString());
        var facility = ActiveFacility(opening: "08:00", closing: "20:00");
        db.Facilities.Add(facility);
        await db.SaveChangesAsync();

        var service = new BookingAvailabilityService(db);
        var errors = await service.ValidateAsync(facility.FacilityId, DateOnly.FromDateTime(DateTime.Now.AddDays(1)), TimeSpan.Parse("19:00"), TimeSpan.Parse("21:00"));

        Assert.Contains(errors, e => e.Contains("operating hours"));
    }

    [Fact]
    public async Task ValidateAsync_RejectsInactiveFacility()
    {
        await using var db = CreateContext(Guid.NewGuid().ToString());
        var facility = ActiveFacility();
        facility.IsActive = false;
        db.Facilities.Add(facility);
        await db.SaveChangesAsync();

        var service = new BookingAvailabilityService(db);
        var errors = await service.ValidateAsync(facility.FacilityId, DateOnly.FromDateTime(DateTime.Now.AddDays(1)), TimeSpan.Parse("10:00"), TimeSpan.Parse("11:00"));

        Assert.Contains(errors, e => e.Contains("not currently available"));
    }

    [Fact]
    public async Task ValidateAsync_RejectsStartTimeNotBeforeEndTime()
    {
        await using var db = CreateContext(Guid.NewGuid().ToString());
        var facility = ActiveFacility();
        db.Facilities.Add(facility);
        await db.SaveChangesAsync();

        var service = new BookingAvailabilityService(db);
        var errors = await service.ValidateAsync(facility.FacilityId, DateOnly.FromDateTime(DateTime.Now.AddDays(1)), TimeSpan.Parse("11:00"), TimeSpan.Parse("10:00"));

        Assert.Contains(errors, e => e.Contains("Start time must be earlier"));
    }

    [Fact]
    public async Task ValidateAsync_RejectsPastDate()
    {
        await using var db = CreateContext(Guid.NewGuid().ToString());
        var facility = ActiveFacility();
        db.Facilities.Add(facility);
        await db.SaveChangesAsync();

        var service = new BookingAvailabilityService(db);
        var errors = await service.ValidateAsync(facility.FacilityId, DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), TimeSpan.Parse("10:00"), TimeSpan.Parse("11:00"));

        Assert.Contains(errors, e => e.Contains("cannot be in the past"));
    }
}
