
// Models/Event.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VenueBooking.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "The Event Name is required.")]
        [StringLength(255, ErrorMessage = "The Event Name must be less than 255 characters.")]
        public string EventName { get; set; }

        [Required(ErrorMessage = "The Description is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "The Event Date is required.")]
        public DateTime EventDate { get; set; }
        public int? VenueId { get; set; }

        public string? ImageUrl { get; set; }
        
        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }     
        [BindNever]   
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

