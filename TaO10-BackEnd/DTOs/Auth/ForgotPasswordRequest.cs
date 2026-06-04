using System.ComponentModel.DataAnnotations;

namespace TaO10_BackEnd.DTOs.Auth
{
 public class ForgotPasswordRequest
 {
 [Required]
 [EmailAddress]
 public string Email { get; set; } = string.Empty;
 }
}
