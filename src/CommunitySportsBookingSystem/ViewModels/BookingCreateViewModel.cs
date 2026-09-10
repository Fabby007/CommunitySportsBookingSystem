using System.ComponentModel.DataAnnotations;

namespace CommunitySportsBookingSystem.ViewModels;

public class BookingCreateViewModel
{
    public int FacilityId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }

    [Required(ErrorMessage = "Booking date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Booking date")]
    public DateOnly? BookingDate { get; set; }

    [Required(ErrorMessage = "Start time is required.")]
    [DataType(DataType.Time)]
    [Display(Name = "Start time")]
    public TimeSpan? StartTime { get; set; }

    [Required(ErrorMessage = "End time is required.")]
    [DataType(DataType.Time)]
    [Display(Name = "End time")]
    public TimeSpan? EndTime { get; set; }
}
