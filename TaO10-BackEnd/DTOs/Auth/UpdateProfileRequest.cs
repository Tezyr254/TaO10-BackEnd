using System.ComponentModel.DataAnnotations;

namespace TaO10_BackEnd.DTOs.Auth
{
    public class UpdateProfileRequest
    {
        public string? FullName { get; set; }

        public string? Phone { get; set; }

        public string? CurrentPassword { get; set; }

        public string? NewPassword { get; set; }

        public string? Location { get; set; }

        public string? Avatar { get; set; }
    }
}
