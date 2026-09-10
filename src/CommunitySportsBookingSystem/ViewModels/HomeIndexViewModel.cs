namespace CommunitySportsBookingSystem.ViewModels;

public class HomeIndexViewModel
{
    public bool IsMember { get; set; }
    public List<FeaturedFacilityViewModel> FeaturedFacilities { get; set; } = new();
    public List<SportSummaryViewModel> Sports { get; set; } = new();
}

/// <summary>Home page facility teaser. Sports/Description are only populated for
/// authenticated members, mirroring the restricted-data convention used by
/// FacilitySearchViewModel's member vs. guest result types.</summary>
public class FeaturedFacilityViewModel
{
    public int FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FacilityType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<string> Sports { get; set; } = new();
}

public class SportSummaryViewModel
{
    public int SportId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FacilityCount { get; set; }
}
