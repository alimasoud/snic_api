using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using snic_api.Models;

namespace snic_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // This requires authentication
    public class ProtectedController : ControllerBase
    {
        [HttpGet("data")]
        public ActionResult<ApiResponse<object>> GetProtectedData()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var data = new
            {
                Message = "This is protected data accessible to all authenticated users!",
                UserId = userId,
                Username = username,
                Email = email,
                Role = role,
                Timestamp = DateTime.UtcNow
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Protected data retrieved successfully",
                Data = data
            });
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public ActionResult<ApiResponse<object>> GetAdminData()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var adminData = new
            {
                Message = "This is admin-only data!",
                AdminUserId = userId,
                AdminUsername = username,
                Role = role,
                ServerTime = DateTime.UtcNow,
                AdminActions = new[]
                {
                    "View all users",
                    "Manage system settings",
                    "Access admin reports",
                    "Moderate content"
                }
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Admin data retrieved successfully",
                Data = adminData
            });
        }

        [HttpGet("customer")]
        [Authorize(Roles = "Customer")]
        public ActionResult<ApiResponse<object>> GetCustomerData()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var customerData = new
            {
                Message = "This is customer-specific data!",
                CustomerId = userId,
                CustomerUsername = username,
                Role = role,
                AccessTime = DateTime.UtcNow,
                AvailableFeatures = new[]
                {
                    "View profile",
                    "Make purchases",
                    "View order history",
                    "Contact support"
                }
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Customer data retrieved successfully",
                Data = customerData
            });
        }
    }
} 