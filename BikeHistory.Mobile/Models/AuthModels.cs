using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BikeHistory.Mobile.Models
{
    public class User
    {
        [Required]
        public string Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Token { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
    }

    public class AuthResponse
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Email { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class ProfileResponse
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Email { get; set; }


        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string>? Roles { get; set; }
    }

    public class UpdateProfileRequest
    {

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        public string ConfirmNewPassword { get; set; }
    }
}