using System.ComponentModel.DataAnnotations;

namespace CommunitySportsBookingSystem.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(50)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(50)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [MaxLength(20)]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required.")]
    [MaxLength(200)]
    [Display(Name = "Address")]
    public string AddressLine { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required.")]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Postcode is required.")]
    [MaxLength(20)]
    public string Postcode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Password and confirmation do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select at least one preferred sport.")]
    [Display(Name = "Preferred sport(s)")]
    public List<int> SelectedSportIds { get; set; } = new();

    public List<SportOption> AvailableSports { get; set; } = new();
}

public class SportOption
{
    public int SportId { get; set; }
    public string Name { get; set; } = string.Empty;
}
