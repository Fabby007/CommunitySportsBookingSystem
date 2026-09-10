namespace CommunitySportsBookingSystem.ViewModels;

public class ReviewBrowseViewModel
{
    public string? FacilityType { get; set; }
    public int? FacilityId { get; set; }
    public int? MinRating { get; set; }

    public List<FacilityOption> AvailableFacilities { get; set; } = new();
    public List<ReviewResultItem> Results { get; set; } = new();
}

public class FacilityOption
{
    public int FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>Public review projection — deliberately excludes any member-identifying field.</summary>
public class ReviewResultItem
{
    public string FacilityName { get; set; } = string.Empty;
    public string FacilityType { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}
