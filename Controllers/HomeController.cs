// Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VenueBooking.Data;
using VenueBooking.Models;

namespace VenueBooking.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string eventName, string venueName, DateTime? startDate, DateTime? endDate)
        {
            var events = await _context.Events
                .Include(e => e.Bookings).ThenInclude(b => b.Venue)
                .Include(e => e.Venue)
                .ToListAsync();

            var eventsWithBookings = events
                .SelectMany(
                    e => e.Bookings.DefaultIfEmpty(),
                    (e, b) => new EventBookingViewModel
                    {
                        EventName = e.EventName,
                        BookingDate = b?.BookingDate,
                        ImageUrl = e.ImageUrl,
                        VenueName = b?.Venue?.VenueName ?? e.Venue?.VenueName ?? "No venue"
                    })
                .Where(x =>
                    (string.IsNullOrEmpty(eventName) || x.EventName.Contains(eventName, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(venueName) || x.VenueName.Contains(venueName, StringComparison.OrdinalIgnoreCase)) &&
                    (!startDate.HasValue || (x.BookingDate.HasValue && x.BookingDate.Value.Date >= startDate.Value.Date)) &&
                    (!endDate.HasValue || (x.BookingDate.HasValue && x.BookingDate.Value.Date <= endDate.Value.Date)))
                .ToList();

            // 🎯 Filter venues too
            var filteredVenues = await _context.Venues
                .Where(v => string.IsNullOrEmpty(venueName) || v.VenueName.ToLower().Contains(venueName.ToLower()))
                .ToListAsync();

            var viewModel = new HomeIndexViewModel
            {
                Events = eventsWithBookings,
                Venues = filteredVenues
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
