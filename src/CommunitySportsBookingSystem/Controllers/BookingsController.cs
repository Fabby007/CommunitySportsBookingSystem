using CommunitySportsBookingSystem.Data;
using CommunitySportsBookingSystem.Models;
using CommunitySportsBookingSystem.Services;
using CommunitySportsBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunitySportsBookingSystem.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IBookingAvailabilityService _availability;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingsController(ApplicationDbContext db, IBookingAvailabilityService availability, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _availability = availability;
        _userManager = userManager;
    }

    [HttpGet("Bookings/Create/{facilityId:int}")]
    public async Task<IActionResult> Create(int facilityId)
    {
        var facility = await _db.Facilities.AsNoTracking().FirstOrDefaultAsync(f => f.FacilityId == facilityId && f.IsActive);
        if (facility is null)
        {
            TempData["ErrorMessage"] = "That facility is not available for booking.";
            return RedirectToAction("Search", "Facilities");
        }

        var vm = new BookingCreateViewModel
        {
            FacilityId = facility.FacilityId,
            FacilityName = facility.Name,
            OpeningTime = facility.OpeningTime,
            ClosingTime = facility.ClosingTime
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel model)
    {
        var facility = await _db.Facilities.AsNoTracking().FirstOrDefaultAsync(f => f.FacilityId == model.FacilityId);
        if (facility is null)
        {
            TempData["ErrorMessage"] = "That facility no longer exists.";
            return RedirectToAction("Search", "Facilities");
        }
        model.FacilityName = facility.Name;
        model.OpeningTime = facility.OpeningTime;
        model.ClosingTime = facility.ClosingTime;

        if (!ModelState.IsValid || model.BookingDate is null || model.StartTime is null || model.EndTime is null)
        {
            return View(model);
        }

        var memberId = _userManager.GetUserId(User)!;

        // Guard against accidental duplicate resubmission of the identical request.
        var duplicate = await _db.Bookings.AnyAsync(b =>
            b.MemberId == memberId
            && b.FacilityId == model.FacilityId
            && b.BookingDate == model.BookingDate
            && b.StartTime == model.StartTime
            && b.EndTime == model.EndTime
            && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed));
        if (duplicate)
        {
            TempData["SuccessMessage"] = "You already have this booking.";
            return RedirectToAction(nameof(Mine));
        }

        var errors = await _availability.ValidateAsync(
            model.FacilityId,
            model.BookingDate.Value,
            model.StartTime.Value,
            model.EndTime.Value);

        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        var booking = new Booking
        {
            MemberId = memberId,
            FacilityId = model.FacilityId,
            BookingDate = model.BookingDate.Value,
            StartTime = model.StartTime.Value,
            EndTime = model.EndTime.Value,
            Status = BookingStatus.Confirmed
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Booking confirmed for {facility.Name} on {model.BookingDate:yyyy-MM-dd}.";
        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    public async Task<IActionResult> Mine()
    {
        var memberId = _userManager.GetUserId(User)!;
        await _availability.PromoteCompletedBookingsAsync(memberId);

        var bookings = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Facility)
            .Include(b => b.Review)
            .Where(b => b.MemberId == memberId)
            .OrderByDescending(b => b.BookingDate).ThenByDescending(b => b.StartTime)
            .Select(b => new BookingListItemViewModel
            {
                BookingId = b.BookingId,
                FacilityName = b.Facility.Name,
                BookingDate = b.BookingDate,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status,
                CanCancel = b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed,
                CanReview = b.Status == BookingStatus.Completed && b.Review == null,
                AlreadyReviewed = b.Review != null
            })
            .ToListAsync();

        return View(bookings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var memberId = _userManager.GetUserId(User)!;
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking is null || booking.MemberId != memberId)
        {
            // Do not reveal whether the booking exists at all if it isn't the caller's.
            return Forbid();
        }

        if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
        {
            TempData["ErrorMessage"] = "Only pending or confirmed bookings can be cancelled.";
            return RedirectToAction(nameof(Mine));
        }

        booking.Status = BookingStatus.Cancelled;
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Booking cancelled.";
        return RedirectToAction(nameof(Mine));
    }
}
