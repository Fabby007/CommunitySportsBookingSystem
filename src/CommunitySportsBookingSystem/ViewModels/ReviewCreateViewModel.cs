using System.ComponentModel.DataAnnotations;

namespace CommunitySportsBookingSystem.ViewModels;

public class ReviewCreateViewModel
{
    public int BookingId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public DateOnly BookingDate { get; set; }

    [Required(ErrorMessage = "Please give a rating.")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Please enter a comment.")]
    [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
    public string Comment { get; set; } = string.Empty;
}
