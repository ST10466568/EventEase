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

        public async Task<IActionResult> Index(string? eventName, string? venueName, DateTime? startDate, DateTime? endDate, int? eventTypeId)
        {
            var eventsQuery = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                    .ThenInclude(e => e.EventType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(eventName))
                eventsQuery = eventsQuery.Where(b => b.Event.EventName.Contains(eventName));

            if (!string.IsNullOrEmpty(venueName))
                eventsQuery = eventsQuery.Where(b => b.Event.Venue.VenueName.Contains(venueName));

            if (eventTypeId.HasValue)
                eventsQuery = eventsQuery.Where(b => b.Event.EventTypeId == eventTypeId);

            if (startDate.HasValue)
                eventsQuery = eventsQuery.Where(b => b.BookingDate >= startDate.Value);

            if (endDate.HasValue)
                eventsQuery = eventsQuery.Where(b => b.BookingDate <= endDate.Value);

            var events = await eventsQuery.Select(b => new EventBookingViewModel
            {
                EventName = b.Event.EventName,
                VenueName = b.Venue != null ? b.Venue.VenueName : "N/A" ,
                BookingDate = b.BookingDate,
                ImageUrl = b.Event.ImageUrl,
                EventTypeName = b.Event.EventType != null ? b.Event.EventType.Name : "N/A"
            }).ToListAsync();

            // --- Logic to filter venues based on full booking within the date range ---
            List<Venue> displayVenues;
            var allVenues = await _context.Venues.ToListAsync();

            if (startDate.HasValue && endDate.HasValue && startDate.Value <= endDate.Value)
            {
                displayVenues = new List<Venue>();
                var datesInSearchRange = Enumerable.Range(0, (endDate.Value.Date - startDate.Value.Date).Days + 1)
                                                   .Select(offset => startDate.Value.Date.AddDays(offset))
                                                   .ToList();

                if (datesInSearchRange.Any()) // Proceed only if the date range is valid and has dates
                {
                    // 1. Fetch relevant booking data (VenueId and distinct BookingDate.Date)
                    var bookingsInDateRangeRaw = await _context.Bookings
                        .Where(b => b.BookingDate.Date >= startDate.Value.Date && b.BookingDate.Date <= endDate.Value.Date)
                        .Select(b => new { b.VenueId, BookingDate = b.BookingDate.Date }) // Select only what's needed
                        .ToListAsync();

                    // 2. Group and process in memory to create the dictionary
                    var venueBookingsInDateRange = bookingsInDateRangeRaw
                        .GroupBy(b => b.VenueId)
                        .ToDictionary(
                            g => g.Key, // VenueId
                            g => new HashSet<DateTime>(g.Select(b => b.BookingDate).Distinct()) // Set of distinct dates
                        );

                    foreach (var venue in allVenues)
                    {
                        // Check if the venue is fully booked for all dates in the search range
                        bool isFullyBooked = venueBookingsInDateRange.TryGetValue(venue.VenueId, out var bookedDatesForVenue) &&
                                             datesInSearchRange.All(dateInSearch => bookedDatesForVenue.Contains(dateInSearch));

                        if (!isFullyBooked)
                        {
                            displayVenues.Add(venue);
                        }
                    }
                }
                else // If date range is invalid or empty, behave as if no range was selected for venues
                {
                    displayVenues = allVenues;
                }
            }
            else // No valid date range provided for filtering venues
            {
                displayVenues = allVenues;
            }

            // --- Further filter displayVenues by venueName if provided ---
            if (!string.IsNullOrEmpty(venueName))
            {
                displayVenues = displayVenues.Where(v => v.VenueName.Contains(venueName, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var model = new HomeIndexViewModel
            {
                Events = events,
                Venues = displayVenues, // Use the filtered list of venues
                EventTypes = await _context.EventTypes.ToListAsync()
            };

            return View(model);
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
