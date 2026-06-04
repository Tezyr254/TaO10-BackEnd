namespace TaO10_BackEnd.DTOs.Auth
{
 public class ProfileResponse
 {
 public Guid UserId { get; set; }
 public string Email { get; set; } = string.Empty;
 public string FullName { get; set; } = string.Empty;
 public string? Phone { get; set; }
 public string? Location { get; set; }
 public string? Avatar { get; set; }
 public string Role { get; set; } = string.Empty;
 public string Status { get; set; } = string.Empty;
 }
}
