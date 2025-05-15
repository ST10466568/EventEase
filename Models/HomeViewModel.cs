// Models/HomeIndexViewModel.cs
using System.Collections.Generic;

namespace VenueBooking.Models
{
    public class HomeIndexViewModel
    {
        public List<EventBookingViewModel> Events { get; set; }
        public List<Venue> Venues { get; set; }
    }
}
