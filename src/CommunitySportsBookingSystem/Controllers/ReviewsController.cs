using CommunitySportsBookingSystem.Data;
using CommunitySportsBookingSystem.Models;
using CommunitySportsBookingSystem.Services;
using CommunitySportsBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunitySportsBookingSystem.Controllers;

public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IBookingAvailabilityService _availability;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewsController(ApplicationDbContext db, IBookingAvailabilityService availability, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _availability = availability;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Browse(ReviewBrowseViewModel criteria)
    {
        criteria.AvailableFacilities = await _db.Facilities
            .OrderBy(f => f.Name)
            .Select(f => new FacilityOption { FacilityId = f.FacilityId, Name = f.Name })
            .ToListAsync();

        var query = _db.Reviews
            .AsNoTracking()
            .Include(r => r.Facility)
            .AsQueryable();

        if (criteria.FacilityId is not null)
        {
            query = query.Where(r => r.FacilityId == criteria.FacilityId);
        }
        if (!string.IsNullOrWhiteSpace(criteria.FacilityType))
        {
            query = query.Where(r => r.Facility.FacilityType.Contains(criteria.FacilityType));
        }
        if (criteria.MinRating is not null)
        {
            query = query.Where(r => r.Rating >= criteria.MinRating);
        }

        criteria.Results = await query
            .OrderByDescending(r => r.SubmittedAt)
            .Select(r => new ReviewResultItem
            {
                FacilityName = r.Facility.Name,
                FacilityType = r.Facility.FacilityType,
                Rating = r.Rating,
                Comment = r.Comment,
                SubmittedAt = r.SubmittedAt
            })
            .ToListAsync();

        return View(criteria);
    }

    [HttpGet("Reviews/Create/{bookingId:int}")]
    [Authorize]
    public async Task<IActionResult> Create(int bookingId)
    {
        var memberId = _userManager.GetUserId(User)!;
        await _availability.PromoteCompletedBookingsAsync(memberId);

        var booking = await _db.Bookings
            .Include(b => b.Facility)
            .Include(b => b.Review)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking is null || booking.MemberId != memberId)
        {
            return Forbid();
        }
        if (booking.Status != BookingStatus.Completed)
        {
            TempData["ErrorMessage"] = "You can only review a facility after your booking is completed.";
            return RedirectToAction("Mine", "Bookings");
        }
        if (booking.Review is not null)
        {
            TempData["ErrorMessage"] = "You have already reviewed this booking.";
            return RedirectToAction("Mine", "Bookings");
        }

        return View(new ReviewCreateViewModel
        {
            BookingId = booking.BookingId,
            FacilityName = booking.Facility.Name,
            BookingDate = booking.BookingDate
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewCreateViewModel model)
    {
        var memberId = _userManager.GetUserId(User)!;

        var booking = await _db.Bookings
            .Include(b => b.Facility)
            .Include(b => b.Review)
            .FirstOrDefaultAsync(b => b.BookingId == model.BookingId);

        if (booking is null || booking.MemberId != memberId)
        {
            return Forbid();
        }

        // Re-validate server-side regardless of what the GET form offered.
        if (booking.Status != BookingStatus.Completed)
        {
            ModelState.AddModelError(string.Empty, "This booking is not eligible for a review yet.");
        }
        if (booking.Review is not null)
        {
            ModelState.AddModelError(string.Empty, "This booking has already been reviewed.");
        }

        if (!ModelState.IsValid)
        {
            model.FacilityName = booking.Facility.Name;
            model.BookingDate = booking.BookingDate;
            return View(model);
        }

        _db.Reviews.Add(new Review
        {
            MemberId = memberId,
            FacilityId = booking.FacilityId,
            BookingId = booking.BookingId,
            Rating = model.Rating,
            Comment = model.Comment
        });
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Thank you — your review has been submitted.";
        return RedirectToAction("Mine", "Bookings");
    }
}
