using System.ComponentModel.DataAnnotations;

namespace CommunitySportsBookingSystem.ViewModels;

public class InquiryCreateViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject is required.")]
    [MaxLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
}
