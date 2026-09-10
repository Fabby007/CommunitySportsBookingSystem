using System.ComponentModel.DataAnnotations;

namespace CommunitySportsBookingSystem.Models;

public class Review
{
    public int ReviewId { get; set; }

    public string MemberId { get; set; } = string.Empty;
    public ApplicationUser Member { get; set; } = null!;

    public int FacilityId { get; set; }
    public Facility Facility { get; set; } = null!;

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
