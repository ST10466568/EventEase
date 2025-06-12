using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
// Models/Booking.cs
namespace VenueBooking.Models
{
    public class Booking
    {

        public int BookingId { get; set; }
        [Required]
        [ForeignKey("EventId")]
        public int EventId { get; set; }
        [JsonIgnore]
        public Event? Event { get; set; }
        [Required]
        public int VenueId { get; set; }
        public Venue? Venue { get; set; }
        public DateTime BookingDate { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
