using System;
using System.Collections.Generic;

namespace TaO10_BackEnd.DTOs.User;

public class UserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string Role { get; set; } = null!;
    public string StatusDisplayName { get; set; } = null!;
    public string StatusCode { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
}

public class UserDetailDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public int? TotalScore { get; set; }
    public int? TotalExams { get; set; }
    public string Role { get; set; } = null!;
    public Guid StatusId { get; set; }
    public string StatusDisplayName { get; set; } = null!;
    public string StatusCode { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateUserStatusRequest
{
    public string StatusCode { get; set; } = null!; // "ACTIVE" or "BLOCKED"
}

public class UpdateUserRoleRequest
{
    public string Role { get; set; } = null!; // "admin" or "customer"
}

public class UserPagedResponse
{
    public List<UserDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
