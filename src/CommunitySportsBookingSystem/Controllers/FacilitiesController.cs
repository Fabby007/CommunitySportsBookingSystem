using CommunitySportsBookingSystem.Data;
using CommunitySportsBookingSystem.Models;
using CommunitySportsBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunitySportsBookingSystem.Controllers;

[AllowAnonymous]
public class FacilitiesController : Controller
{
    private readonly ApplicationDbContext _db;

    public FacilitiesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Search(FacilitySearchViewModel criteria)
    {
        criteria.AvailableSports = await _db.Sports
            .OrderBy(s => s.Name)
            .Select(s => new SportOption { SportId = s.SportId, Name = s.Name })
            .ToListAsync();

        var query = _db.Facilities
            .AsNoTracking()
            .Include(f => f.FacilitySports).ThenInclude(fs => fs.Sport)
            .Where(f => f.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.FacilityType))
        {
            query = query.Where(f => f.FacilityType.Contains(criteria.FacilityType));
        }
        if (!string.IsNullOrWhiteSpace(criteria.Location))
        {
            query = query.Where(f => f.Location.Contains(criteria.Location));
        }
        if (criteria.SportId is not null)
        {
            query = query.Where(f => f.FacilitySports.Any(fs => fs.SportId == criteria.SportId));
        }
        if (criteria.StartTime is not null)
        {
            query = query.Where(f => f.OpeningTime <= criteria.StartTime);
        }
        if (criteria.EndTime is not null)
        {
            query = query.Where(f => f.ClosingTime >= criteria.EndTime);
        }

        var facilities = await query.ToListAsync();
        var queryDate = criteria.Date ?? DateOnly.FromDateTime(DateTime.Now);

        criteria.IsMember = User.Identity?.IsAuthenticated == true;

        if (criteria.IsMember)
        {
            criteria.MemberResults = new List<FacilitySearchResultViewModel>();
            foreach (var f in facilities)
            {
                criteria.MemberResults.Add(new FacilitySearchResultViewModel
                {
                    FacilityId = f.FacilityId,
                    Name = f.Name,
                    FacilityType = f.FacilityType,
                    Location = f.Location,
                    Description = f.Description,
                    Amenities = f.Amenities,
                    OpeningTime = f.OpeningTime,
                    ClosingTime = f.ClosingTime,
                    HasAvailability = await HasAvailabilityAsync(f, queryDate),
                    Sports = f.FacilitySports.Select(fs => fs.Sport.Name).OrderBy(n => n).ToList()
                });
            }
        }
        else
        {
            criteria.GuestResults = new List<GuestFacilitySearchResultViewModel>();
            foreach (var f in facilities)
            {
                criteria.GuestResults.Add(new GuestFacilitySearchResultViewModel
                {
                    FacilityId = f.FacilityId,
                    Name = f.Name,
                    FacilityType = f.FacilityType,
                    GeneralLocation = f.Location,
                    HasAvailability = await HasAvailabilityAsync(f, queryDate)
                });
            }
        }

        return View(criteria);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var facility = await _db.Facilities
            .AsNoTracking()
            .Include(f => f.FacilitySports).ThenInclude(fs => fs.Sport)
            .FirstOrDefaultAsync(f => f.FacilityId == id && f.IsActive);

        if (facility is null)
        {
            return NotFound();
        }

        var today = DateOnly.FromDateTime(DateTime.Now);
        var isMember = User.Identity?.IsAuthenticated == true;

        if (isMember)
        {
            var vm = new FacilitySearchResultViewModel
            {
                FacilityId = facility.FacilityId,
                Name = facility.Name,
                FacilityType = facility.FacilityType,
                Location = facility.Location,
                Description = facility.Description,
                Amenities = facility.Amenities,
                OpeningTime = facility.OpeningTime,
                ClosingTime = facility.ClosingTime,
                HasAvailability = await HasAvailabilityAsync(facility, today),
                Sports = facility.FacilitySports.Select(fs => fs.Sport.Name).OrderBy(n => n).ToList()
            };
            ViewData["IsMember"] = true;
            return View("Details", vm);
        }
        else
        {
            var vm = new GuestFacilitySearchResultViewModel
            {
                FacilityId = facility.FacilityId,
                Name = facility.Name,
                FacilityType = facility.FacilityType,
                GeneralLocation = facility.Location,
                HasAvailability = await HasAvailabilityAsync(facility, today)
            };
            ViewData["IsMember"] = false;
            return View("GuestDetails", vm);
        }
    }

    private async Task<bool> HasAvailabilityAsync(Facility facility, DateOnly date)
    {
        var totalMinutes = (facility.ClosingTime - facility.OpeningTime).TotalMinutes;
        if (totalMinutes <= 0)
        {
            return false;
        }

        var activeBookings = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.FacilityId == facility.FacilityId
                && b.BookingDate == date
                && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed))
            .Select(b => new { b.StartTime, b.EndTime })
            .ToListAsync();

        if (activeBookings.Count == 0)
        {
            return true;
        }

        var intervals = activeBookings
            .Select(b => (Start: b.StartTime, End: b.EndTime))
            .OrderBy(i => i.Start)
            .ToList();

        var mergedMinutes = 0.0;
        var currentStart = intervals[0].Start;
        var currentEnd = intervals[0].End;

        for (var i = 1; i < intervals.Count; i++)
        {
            if (intervals[i].Start <= currentEnd)
            {
                if (intervals[i].End > currentEnd)
                {
                    currentEnd = intervals[i].End;
                }
            }
            else
            {
                mergedMinutes += (currentEnd - currentStart).TotalMinutes;
                currentStart = intervals[i].Start;
                currentEnd = intervals[i].End;
            }
        }
        mergedMinutes += (currentEnd - currentStart).TotalMinutes;

        return mergedMinutes < totalMinutes;
    }
}
