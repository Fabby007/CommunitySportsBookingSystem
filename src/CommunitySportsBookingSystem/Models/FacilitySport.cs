namespace CommunitySportsBookingSystem.Models;

public class FacilitySport
{
    public int FacilityId { get; set; }
    public Facility Facility { get; set; } = null!;

    public int SportId { get; set; }
    public Sport Sport { get; set; } = null!;
}
