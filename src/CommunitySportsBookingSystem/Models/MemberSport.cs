namespace CommunitySportsBookingSystem.Models;

public class MemberSport
{
    public string MemberId { get; set; } = string.Empty;
    public ApplicationUser Member { get; set; } = null!;

    public int SportId { get; set; }
    public Sport Sport { get; set; } = null!;
}
