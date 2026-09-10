using CommunitySportsBookingSystem.Data;
using CommunitySportsBookingSystem.Models;
using CommunitySportsBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunitySportsBookingSystem.Controllers;

[AllowAnonymous]
public class InquiriesController : Controller
{
    private readonly ApplicationDbContext _db;

    public InquiriesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new InquiryCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InquiryCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _db.Inquiries.Add(new Inquiry
        {
            Name = model.Name,
            Email = model.Email,
            Subject = model.Subject,
            Message = model.Message,
            Status = "New"
        });
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Confirmation));
    }

    [HttpGet]
    public IActionResult Confirmation()
    {
        return View();
    }
}
