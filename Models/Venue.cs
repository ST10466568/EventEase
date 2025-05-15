
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Added for ValidateNever
using System.Text.Json.Serialization; // Added for JsonIgnore

namespace VenueBooking.Models
{
    public class Venue
    {
        public int VenueId { get; set; }
        public string VenueName { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public string? ImageUrl { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        [BindNever]
        [ValidateNever] // Prevent model validation on this collection
        [JsonIgnore]     // Optional: prevents API loops
        public ICollection<Booking> Bookings { get; set; }
    }
}