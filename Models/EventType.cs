// Models/EventType.cs
using System.ComponentModel.DataAnnotations;

namespace VenueBooking.Models
{
    public class EventType
    {
        public int EventTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Optional: Navigation property for events
        public List<Event> Events { get; set; } = new();
    }
}
