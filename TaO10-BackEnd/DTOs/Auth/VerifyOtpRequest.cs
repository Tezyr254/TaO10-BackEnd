using System.ComponentModel.DataAnnotations;

namespace TaO10_BackEnd.DTOs.Auth
{
 public class VerifyOtpRequest
 {
 [Required]
 [EmailAddress]
 public string Email { get; set; } = string.Empty;

 [Required]
 [MinLength(6), MaxLength(6)]
 [RegularExpression("^[0-9]{6}$")]
 public string Otp { get; set; } = string.Empty;
 }
}
