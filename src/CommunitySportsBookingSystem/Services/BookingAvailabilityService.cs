using CommunitySportsBookingSystem.Data;
using CommunitySportsBookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunitySportsBookingSystem.Services;

public class BookingAvailabilityService : IBookingAvailabilityService
{
    private readonly ApplicationDbContext _db;

    public BookingAvailabilityService(ApplicationDbContext db)
    {
        _db = db;
    }

    public bool RangesOverlap(TimeSpan aStart, TimeSpan aEnd, TimeSpan bStart, TimeSpan bEnd)
    {
        // Half-open interval overlap test: boundary-touching ranges do NOT conflict.
        return aStart < bEnd && bStart < aEnd;
    }

    public async Task<IReadOnlyList<string>> ValidateAsync(
        int facilityId,
        DateOnly bookingDate,
        TimeSpan startTime,
        TimeSpan endTime,
        int? excludeBookingId = null,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (startTime >= endTime)
        {
            errors.Add("Start time must be earlier than end time.");
        }

        if (bookingDate < DateOnly.FromDateTime(DateTime.Now))
        {
            errors.Add("Booking date cannot be in the past.");
        }

        var facility = await _db.Facilities
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FacilityId == facilityId, cancellationToken);

        if (facility is null)
        {
            errors.Add("The selected facility does not exist.");
            return errors;
        }

        if (!facility.IsActive)
        {
            errors.Add("The selected facility is not currently available for booking.");
        }

        if (startTime < facility.OpeningTime || endTime > facility.ClosingTime)
        {
            errors.Add($"Booking time must fall within the facility's operating hours ({facility.OpeningTime:hh\\:mm}-{facility.ClosingTime:hh\\:mm}).");
        }

        // Only re-check overlap if the basic range/facility checks so far are sound enough to be meaningful.
        if (startTime < endTime)
        {
            var activeBookingsSameDay = await _db.Bookings
                .AsNoTracking()
                .Where(b => b.FacilityId == facilityId
                    && b.BookingDate == bookingDate
                    && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                    && (excludeBookingId == null || b.BookingId != excludeBookingId))
                .Select(b => new { b.StartTime, b.EndTime })
                .ToListAsync(cancellationToken);

            var hasConflict = activeBookingsSameDay.Any(b => RangesOverlap(startTime, endTime, b.StartTime, b.EndTime));
            if (hasConflict)
            {
                errors.Add("This facility already has a booking that overlaps the requested time.");
            }
        }

        return errors;
    }

    public async Task PromoteCompletedBookingsAsync(string? memberId = null, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var timeOfDay = now.TimeOfDay;

        var query = _db.Bookings.Where(b =>
            (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
            && (b.BookingDate < today || (b.BookingDate == today && b.EndTime <= timeOfDay)));

        if (memberId is not null)
        {
            query = query.Where(b => b.MemberId == memberId);
        }

        await query.ExecuteUpdateAsync(
            setters => setters.SetProperty(b => b.Status, BookingStatus.Completed),
            cancellationToken);
    }
}
