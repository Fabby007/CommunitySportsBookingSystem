namespace CommunitySportsBookingSystem.Models;

public class Booking
{
    public int BookingId { get; set; }

    public string MemberId { get; set; } = string.Empty;
    public ApplicationUser Member { get; set; } = null!;

    public int FacilityId { get; set; }
    public Facility Facility { get; set; } = null!;

    public DateOnly BookingDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Review? Review { get; set; }
}
