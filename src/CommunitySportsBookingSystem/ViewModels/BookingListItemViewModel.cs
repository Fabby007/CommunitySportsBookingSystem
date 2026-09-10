using CommunitySportsBookingSystem.Models;

namespace CommunitySportsBookingSystem.ViewModels;

public class BookingListItemViewModel
{
    public int BookingId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public DateOnly BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public BookingStatus Status { get; set; }
    public bool CanCancel { get; set; }
    public bool CanReview { get; set; }
    public bool AlreadyReviewed { get; set; }
}
