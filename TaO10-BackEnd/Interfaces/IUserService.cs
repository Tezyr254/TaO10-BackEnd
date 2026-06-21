using System;
using System.Threading.Tasks;
using TaO10_BackEnd.DTOs.User;

namespace TaO10_BackEnd.Interfaces;

public interface IUserService
{
    Task<UserPagedResponse> GetPagedUsersAsync(string? searchTerm, int pageNumber, int pageSize);
    Task<UserDetailDto> GetUserByIdAsync(Guid userId);
    Task<bool> UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequest request);
    Task<bool> UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request);
}
