// c:\Projects\VenueBooking\ViewModels\ResetPasswordViewModel.cs
namespace VenueBooking.Models
{
    public class ResetPasswordViewModel
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
