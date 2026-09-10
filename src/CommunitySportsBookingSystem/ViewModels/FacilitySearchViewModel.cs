namespace CommunitySportsBookingSystem.ViewModels;

public class FacilitySearchViewModel
{
    public string? FacilityType { get; set; }
    public int? SportId { get; set; }
    public string? Location { get; set; }
    public DateOnly? Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    public List<SportOption> AvailableSports { get; set; } = new();

    public bool IsMember { get; set; }
    public List<FacilitySearchResultViewModel> MemberResults { get; set; } = new();
    public List<GuestFacilitySearchResultViewModel> GuestResults { get; set; } = new();
}

/// <summary>Full-detail result shown only to authenticated members (FR-011).</summary>
public class FacilitySearchResultViewModel
{
    public int FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FacilityType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Amenities { get; set; }
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public bool HasAvailability { get; set; }
    public List<string> Sports { get; set; } = new();
}

/// <summary>Restricted result shown to guests (FR-012) — deliberately excludes
/// Description/Amenities/exact-slot detail so the extra fields never reach an
/// unauthenticated response, not just hidden in the view.</summary>
public class GuestFacilitySearchResultViewModel
{
    public int FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FacilityType { get; set; } = string.Empty;
    public string GeneralLocation { get; set; } = string.Empty;
    public bool HasAvailability { get; set; }
}
