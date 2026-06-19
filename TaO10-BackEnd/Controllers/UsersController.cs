using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaO10_BackEnd.Common;
using TaO10_BackEnd.DTOs.User;
using TaO10_BackEnd.Interfaces;

namespace TaO10_BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    private bool IsAdmin()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
        return roleClaim != null && string.Equals(roleClaim.Value, "admin", StringComparison.OrdinalIgnoreCase);
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedUsers([FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        if (!IsAdmin()) return Forbid();

        try
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<UserPagedResponse>.ErrorResponse(
                    "Trang và số lượng bản ghi phải lớn hơn 0",
                    "INVALID_PAGINATION",
                    400));
            }

            var result = await _userService.GetPagedUsersAsync(search, pageNumber, pageSize);
            return Ok(ApiResponse<UserPagedResponse>.SuccessResponse(result, "Lấy danh sách người dùng thành công", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPagedUsers");
            return StatusCode(500, ApiResponse<UserPagedResponse>.ErrorResponse("Lỗi hệ thống khi tải danh sách người dùng", "INTERNAL_ERROR", 500));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        if (!IsAdmin()) return Forbid();

        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(ApiResponse<UserDetailDto>.SuccessResponse(user, "Lấy chi tiết người dùng thành công", 200));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserDetailDto>.ErrorResponse(ex.Message, "USER_NOT_FOUND", 404));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserById");
            return StatusCode(500, ApiResponse<UserDetailDto>.ErrorResponse("Lỗi hệ thống khi tải chi tiết người dùng", "INTERNAL_ERROR", 500));
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        if (!IsAdmin()) return Forbid();

        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.StatusCode))
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Trạng thái không hợp lệ", "INVALID_STATUS", 400));
            }

            var success = await _userService.UpdateUserStatusAsync(id, request);
            if (success)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Cập nhật trạng thái người dùng thành công", 200));
            }
            return BadRequest(ApiResponse<bool>.ErrorResponse("Không thể cập nhật trạng thái người dùng", "UPDATE_FAILED", 400));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message, "USER_NOT_FOUND", 404));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message, "INVALID_ARGUMENT", 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateUserStatus");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi hệ thống khi cập nhật trạng thái", "INTERNAL_ERROR", 500));
        }
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequest request)
    {
        if (!IsAdmin()) return Forbid();

        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Quyền không hợp lệ", "INVALID_ROLE", 400));
            }

            var success = await _userService.UpdateUserRoleAsync(id, request);
            if (success)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Cập nhật vai trò người dùng thành công", 200));
            }
            return BadRequest(ApiResponse<bool>.ErrorResponse("Không thể cập nhật vai trò người dùng", "UPDATE_FAILED", 400));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message, "USER_NOT_FOUND", 404));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message, "INVALID_ARGUMENT", 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateUserRole");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi hệ thống khi cập nhật vai trò", "INTERNAL_ERROR", 500));
        }
    }
}
