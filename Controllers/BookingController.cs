// Controllers/BookingController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VenueBooking.Data;
using VenueBooking.Models;

namespace VenueBooking.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // Change here

        public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) // Change here
        {
            _context = context;
            _userManager = userManager; // Change here
        }

        // GET: Booking/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (await IsDuplicateBooking(booking))
            {
                ModelState.AddModelError(string.Empty, "A booking already exists for this venue on the selected date.");
            }

            if (ModelState.IsValid)
            {
                booking.CreatedBy = User.Identity?.Name ?? "Anonymous";
                booking.CreatedDate = DateTime.Now;

                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(booking);
        }

        // GET: Booking/Index
        public async Task<IActionResult> Index()
        {
            return View(await _context.Bookings.Include(b => b.Event).Include(b => b.Venue).ToListAsync());
        }

        // GET: Booking/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingId)
                return NotFound();

            if (await IsDuplicateBooking(booking, excludeBookingId: id))
            {
                ModelState.AddModelError(string.Empty, "A booking already exists for this venue on the selected date.");
            }

            if (ModelState.IsValid)
            {
                booking.ModifiedBy = User.Identity?.Name ?? "Anonymous";
                booking.ModifiedDate = DateTime.Now;

                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(booking);
        }
        // GET: Booking/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Booking/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }

        private async Task<bool> IsDuplicateBooking(Booking booking, int? excludeBookingId = null)
        {
            return await _context.Bookings.AnyAsync(b =>
                b.BookingDate.Date == booking.BookingDate.Date &&
                b.VenueId == booking.VenueId &&
                (excludeBookingId == null || b.BookingId != excludeBookingId));
        }
    }

    
}
