using System.ComponentModel.DataAnnotations;

namespace TaO10_BackEnd.DTOs.Auth
{
 public class ResetPasswordRequest
 {
 [Required]
 public string ResetToken { get; set; } = string.Empty;

 [Required]
 [MinLength(8)]
 public string NewPassword { get; set; } = string.Empty;

 [Required]
 [Compare("NewPassword")]
 public string ConfirmPassword { get; set; } = string.Empty;
 }
}
