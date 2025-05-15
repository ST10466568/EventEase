// c:\Projects\VenueBooking\Models\ForgotPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace VenueBooking.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
