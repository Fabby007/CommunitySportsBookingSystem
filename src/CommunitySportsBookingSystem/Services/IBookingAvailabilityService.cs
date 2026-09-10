using CommunitySportsBookingSystem.Models;

namespace CommunitySportsBookingSystem.Services;

public interface IBookingAvailabilityService
{
    /// <summary>
    /// Validates a candidate booking against facility state, operating hours, and
    /// conflicting active bookings. Returns an empty list if the booking is valid.
    /// </summary>
    Task<IReadOnlyList<string>> ValidateAsync(
        int facilityId,
        DateOnly bookingDate,
        TimeSpan startTime,
        TimeSpan endTime,
        int? excludeBookingId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pure overlap test: two [start,end) ranges on the same facility/date conflict
    /// only if they genuinely overlap; ranges that merely touch at a boundary do not.
    /// </summary>
    bool RangesOverlap(TimeSpan aStart, TimeSpan aEnd, TimeSpan bStart, TimeSpan bEnd);

    /// <summary>
    /// Transitions any Pending/Confirmed booking whose date+end time has already
    /// passed to Completed. Applied lazily (no background job) whenever a member's
    /// bookings are read, so Status reflects reality without extra infrastructure.
    /// </summary>
    Task PromoteCompletedBookingsAsync(string? memberId = null, CancellationToken cancellationToken = default);
}
