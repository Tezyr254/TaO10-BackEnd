using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.DTOs.User;
using TaO10_BackEnd.Interfaces;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserPagedResponse> GetPagedUsersAsync(string? searchTerm, int pageNumber, int pageSize)
    {
        var query = _context.Users
            .Include(u => u.Status)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim();
            query = query.Where(u => EF.Functions.ILike(u.FullName, $"%{search}%") || EF.Functions.ILike(u.Email, $"%{search}%"));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                Role = u.Role,
                StatusCode = u.Status != null ? u.Status.Code : string.Empty,
                StatusDisplayName = u.Status != null ? u.Status.DisplayName : string.Empty,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return new UserPagedResponse
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<UserDetailDto> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Status)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            throw new KeyNotFoundException("Không tìm thấy người dùng.");
        }

        return new UserDetailDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            Avatar = user.Avatar,
            Phone = user.Phone,
            Location = user.Location,
            TotalScore = user.TotalScore,
            TotalExams = user.TotalExams,
            Role = user.Role,
            StatusId = user.StatusId,
            StatusCode = user.Status != null ? user.Status.Code : string.Empty,
            StatusDisplayName = user.Status != null ? user.Status.DisplayName : string.Empty,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public async Task<bool> UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            throw new KeyNotFoundException("Không tìm thấy người dùng.");
        }

        var status = await _context.Statuses
            .FirstOrDefaultAsync(s => s.EntityType == "User" && EF.Functions.ILike(s.Code, request.StatusCode));

        if (status == null)
        {
            throw new ArgumentException($"Không tìm thấy trạng thái tương thích: {request.StatusCode}");
        }

        user.StatusId = status.StatusId;
        user.UpdatedAt = DateTime.UtcNow;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            throw new KeyNotFoundException("Không tìm thấy người dùng.");
        }

        var normalizedRole = request.Role.Trim().ToLower();
        if (normalizedRole != "admin" && normalizedRole != "customer")
        {
            throw new ArgumentException("Quyền truy cập không hợp lệ. Chỉ chấp nhận 'admin' hoặc 'customer'.");
        }

        user.Role = normalizedRole;
        user.UpdatedAt = DateTime.UtcNow;

        return await _context.SaveChangesAsync() > 0;
    }
}
