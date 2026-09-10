using System.ComponentModel.DataAnnotations;

namespace CommunitySportsBookingSystem.Models;

public class Inquiry
{
    public int InquiryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "New";

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
