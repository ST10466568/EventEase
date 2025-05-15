using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VenueBooking.Data;
using VenueBooking.Models;

namespace VenueBooking.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // Change here
        private readonly BlobStorageService _blobService;

        public EventController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, BlobStorageService blobService) // Change here
        {
            _context = context;
            _userManager = userManager; // Change here
            _blobService = blobService;
        }


         // --- Helper method to populate the ViewBag (Keep this if you added it before) ---
        private async Task PopulateVenuesViewBag(object selectedVenue = null)
        {
            var venuesQuery = from v in _context.Venues // Assuming your Venue DbSet is called Venues
                            orderby v.VenueName // Or however you want to order them
                            select v;

            // Assign the SelectList to ViewBag.VenueId
            ViewBag.VenueId = new SelectList(await venuesQuery.AsNoTracking().ToListAsync(),
                                            "VenueId", // The property for the option value
                                            "VenueName",    // The property for the option text
                                            selectedVenue); // Optional: pre-select an item
        }

        // GET: Event/Create
        public async Task<IActionResult> Create()
        {
            await PopulateVenuesViewBag();
            var model = new Event { Bookings = new List<Booking>() };
            return View(model);
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventItem, string bookingsJson, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string fileUrl = await _blobService.UploadFileAsync(file, fileName);
                Console.WriteLine("Uploaded to blob: " + fileUrl);
                eventItem.ImageUrl = fileUrl;
            }

            eventItem.CreatedBy = User.Identity?.Name ?? "Anonymous";
            eventItem.CreatedDate = DateTime.Now;

            var bookings = !string.IsNullOrEmpty(bookingsJson)
                ? JsonConvert.DeserializeObject<List<Booking>>(bookingsJson)
                : new List<Booking>();

            if (ModelState.IsValid)
            {
                foreach (var booking in bookings)
                {
                    if (await IsBookingConflictAsync(booking.VenueId, booking.BookingDate))
                    {
                        ModelState.AddModelError("Bookings", $"A booking already exists for this venue on {booking.BookingDate.ToShortDateString()}.");
                        await PopulateVenuesViewBag(eventItem.VenueId);
                        return View(eventItem);
                    }
                }

                _context.Add(eventItem);
                await _context.SaveChangesAsync();

                foreach (var booking in bookings)
                {
                    booking.EventId = eventItem.EventId;
                    booking.CreatedBy = User.Identity?.Name ?? "Anonymous";
                    booking.CreatedDate = DateTime.Now;

                    _context.Bookings.Add(booking);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateVenuesViewBag(eventItem.VenueId);
            return View(eventItem);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var eventItem = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound();

            await PopulateVenuesViewBag(eventItem.VenueId);
            return View(eventItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventItem, string bookingsJson, IFormFile file)
        {
            if (id != eventItem.EventId)
                return NotFound();

            if (file != null && file.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string fileUrl = await _blobService.UploadFileAsync(file, fileName);
                eventItem.ImageUrl = fileUrl;
            }

            eventItem.ModifiedBy = User.Identity?.Name ?? "Anonymous";
            eventItem.ModifiedDate = DateTime.Now;

            var bookings = !string.IsNullOrEmpty(bookingsJson)
                ? JsonConvert.DeserializeObject<List<Booking>>(bookingsJson)
                : new List<Booking>();

            if (ModelState.IsValid)
            {
                foreach (var booking in bookings)
                {
                    if (await IsBookingConflictAsync(booking.VenueId, booking.BookingDate, booking.BookingId))
                    {
                        ModelState.AddModelError("Bookings", $"A booking already exists for this venue on {booking.BookingDate.ToShortDateString()}.");
                        await PopulateVenuesViewBag(eventItem.VenueId);
                        return View(eventItem);
                    }
                }

                _context.Update(eventItem);
                await _context.SaveChangesAsync();

                foreach (var booking in bookings)
                {
                    booking.EventId = eventItem.EventId;
                    booking.CreatedBy ??= User.Identity?.Name ?? "Anonymous";
                    booking.ModifiedBy = User.Identity?.Name ?? "Anonymous";
                    booking.ModifiedDate = DateTime.Now;

                    if (booking.BookingId == 0)
                        _context.Bookings.Add(booking);
                    else
                        _context.Update(booking);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateVenuesViewBag(eventItem.VenueId);
            return View(eventItem);
        }

        // GET: Event/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }
        
        // POST: Event/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound();

            // Prevent deletion if bookings exist and user is Admin
            if ((User.IsInRole("Admin") || User.IsInRole("Booking Specialist")) && eventItem.Bookings.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete event. It has existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.Include(e => e.Venue).ToListAsync();
            return View(events);
        }

        [HttpGet]
        public async Task<IActionResult> HasBookings(int id)
        {
            var ev = await _context.Events.Include(e => e.Bookings).FirstOrDefaultAsync(e => e.EventId == id);
            bool hasBookings = ev?.Bookings?.Any() ?? false;
            return Json(new { hasBookings });
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }

            // Action to delete a specific booking
    [HttpPost] // Or [HttpDelete] if you prefer and configure routing/AJAX accordingly
    // Consider adding [ValidateAntiForgeryToken] for security if needed, and include token in AJAX headers
    public async Task<IActionResult> DeleteBooking(int id) // 'id' matches the parameter in the AJAX URL
    {
        if (id <= 0)
        {
            return BadRequest(new { success = false, message = "Invalid Booking ID." });
        }

        var booking = await _context.Bookings.FindAsync(id); // Find the booking by primary key

        if (booking == null)
        {
            // You might return NotFound or Ok with a specific message
            // depending on how you want the client to react if it's already deleted.
            return NotFound(new { success = false, message = "Booking not found." });
        }

        // Optional: Add authorization checks here if needed
        // if (!UserCanDeleteBooking(booking)) return Forbid();

        try
        {
            _context.Bookings.Remove(booking); // Mark for deletion
            await _context.SaveChangesAsync(); // Execute deletion in DB
            return Ok(new { success = true, message = "Booking deleted successfully." }); // Send success response
        }
        catch (DbUpdateException dbEx)
        {
            // Log the detailed database error (dbEx)
            return StatusCode(500, new { success = false, message = "Database error occurred while deleting the booking." });
        }
        catch (Exception ex)
        {
            // Log the general error (ex)
            return StatusCode(500, new { success = false, message = "An unexpected error occurred." });
        }
    }

        private async Task<bool> IsBookingConflictAsync(int venueId, DateTime bookingDate, int? bookingIdToIgnore = null)
        {
            var existingBooking = await _context.Bookings
                .Where(b => b.VenueId == venueId && b.BookingDate.Date == bookingDate.Date && (bookingIdToIgnore == null || b.BookingId != bookingIdToIgnore))
                .FirstOrDefaultAsync();

            return existingBooking != null;
        }
    }
}
