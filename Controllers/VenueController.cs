

// Controllers/VenueController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VenueBooking.Data;
using VenueBooking.Models;

[Authorize]
public class VenueController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly BlobStorageService _blobService;

    public VenueController(ApplicationDbContext context, BlobStorageService blobService)
    {
        _context = context;
        _blobService = blobService;
    }

    // GET: Venue/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Venue/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Venue venue, IFormFile file)
    {
        if (ModelState.IsValid)
        {
            if (file != null && file.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string fileUrl = await _blobService.UploadFileAsync(file, fileName);
                venue.ImageUrl = fileUrl;
            }

            // Set CreatedBy here
            if (User.Identity.IsAuthenticated)
            {
                venue.CreatedBy = User.Identity.Name;
            }
            else
            {
                venue.CreatedBy = "Anonymous"; // Or "System" or any other default value
            }
            _context.Add(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(venue);
    }

    // GET: Venue/Edit
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var venue = await _context.Venues.FindAsync(id);
        if (venue == null)
        {
            return NotFound();
        }
        return View(venue);
    }

    // POST: Venue/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Venue venue, IFormFile file)
    {
        if (id != venue.VenueId)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string fileUrl = await _blobService.UploadFileAsync(file, fileName);
                    venue.ImageUrl = fileUrl;
                }

                venue.ModifiedBy = User.Identity?.Name ?? "Anonymous";
                venue.ModifiedDate = DateTime.Now;

                _context.Update(venue);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Venues.Any(v => v.VenueId == venue.VenueId))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(venue);
    }

    // GET: Venue/Delete
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var venue = await _context.Venues
            .Include(v => v.Bookings)
            .FirstOrDefaultAsync(v => v.VenueId == id);

        if (venue == null)
            return NotFound();

        // ✅ Prevent deletion if venue has bookings
        if (venue.Bookings.Any())
        {
            TempData["ErrorMessage"] = "Cannot delete this venue. It has existing bookings.";
            return RedirectToAction(nameof(Index));
        }

        _context.Venues.Remove(venue);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> HasBookings(int id)
    {
        var venue = await _context.Venues
            .Include(v => v.Bookings)
            .FirstOrDefaultAsync(v => v.VenueId == id);

        bool hasBookings = venue?.Bookings?.Any() ?? false;

        return Json(new { hasBookings });
    }

    // GET: Venue/Index
    public async Task<IActionResult> Index()
    {
        return View(await _context.Venues.ToListAsync());
    }

    private bool VenueExists(int id)
    {
        return _context.Venues.Any(e => e.VenueId == id);
    }
}
