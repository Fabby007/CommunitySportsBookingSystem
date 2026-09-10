using System.ComponentModel.DataAnnotations;

namespace CommunitySportsBookingSystem.Models;

public class Sport
{
    public int SportId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<MemberSport> MemberSports { get; set; } = new List<MemberSport>();
    public ICollection<FacilitySport> FacilitySports { get; set; } = new List<FacilitySport>();
}
