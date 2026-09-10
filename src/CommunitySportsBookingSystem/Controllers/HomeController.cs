using System.Diagnostics;
using CommunitySportsBookingSystem.Data;
using CommunitySportsBookingSystem.Models;
using CommunitySportsBookingSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunitySportsBookingSystem.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var isMember = User.Identity?.IsAuthenticated == true;

        var facilities = await _db.Facilities
            .AsNoTracking()
            .Include(f => f.FacilitySports).ThenInclude(fs => fs.Sport)
            .Where(f => f.IsActive)
            .OrderBy(f => f.Name)
            .Take(6)
            .ToListAsync();

        var vm = new HomeIndexViewModel
        {
            IsMember = isMember,
            FeaturedFacilities = facilities.Select(f => new FeaturedFacilityViewModel
            {
                FacilityId = f.FacilityId,
                Name = f.Name,
                FacilityType = f.FacilityType,
                Location = f.Location,
                Sports = isMember
                    ? f.FacilitySports.Select(fs => fs.Sport.Name).OrderBy(n => n).ToList()
                    : new List<string>()
            }).ToList(),
            Sports = await _db.Sports
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new SportSummaryViewModel
                {
                    SportId = s.SportId,
                    Name = s.Name,
                    FacilityCount = s.FacilitySports.Count(fs => fs.Facility.IsActive)
                })
                .ToListAsync()
        };

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
