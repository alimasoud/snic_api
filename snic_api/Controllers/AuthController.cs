using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snic_api.Models;
using snic_api.Services;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace snic_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly TokenBlacklistService _tokenBlacklistService;

        public AuthController(ApplicationDbContext context, JwtService jwtService, TokenBlacklistService tokenBlacklistService)
        {
            _context = context;
            _jwtService = jwtService;
            _tokenBlacklistService = tokenBlacklistService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(RegisterRequest request)
        {
            try
            {
                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest(new ApiResponse<AuthResponse>
                    {
                        Success = false,
                        Message = "Email already registered"
                    });
                }

                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                {
                    return BadRequest(new ApiResponse<AuthResponse>
                    {
                        Success = false,
                        Message = "Username already taken"
                    });
                }

                // Create new user
                var user = new User
                {
                    Email = request.Email,
                    Username = request.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = request.Role,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate token
                var token = _jwtService.GenerateToken(user);

                var response = new AuthResponse
                {
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };

                return Ok(new ApiResponse<AuthResponse>
                {
                    Success = true,
                    Message = "User registered successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "An error occurred during registration"
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request)
        {
            try
            {
                // Find user by username
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null)
                {
                    return BadRequest(new ApiResponse<AuthResponse>
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    });
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return BadRequest(new ApiResponse<AuthResponse>
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    });
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Generate token
                var token = _jwtService.GenerateToken(user);

                var response = new AuthResponse
                {
                    userId = user.Id.ToString(),
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };

                return Ok(new ApiResponse<AuthResponse>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> Logout()
        {
            try
            {
                // Extract token from Authorization header
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader == null || !authHeader.StartsWith("Bearer "))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No valid token provided"
                    });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                
                // Get user ID from token claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid token"
                    });
                }

                // Blacklist the token
                await _tokenBlacklistService.BlacklistTokenAsync(token, userId, "User logout");

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Logout successful - token has been invalidated"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during logout"
                });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid token"
                    });
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                var profile = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    Role = user.Role.ToString(),
                    user.CreatedAt,
                    user.LastLoginAt
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Profile retrieved successfully",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving profile"
                });
            }
        }

        [HttpGet("token-status")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> GetTokenStatus()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader == null || !authHeader.StartsWith("Bearer "))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No valid token provided"
                    });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var isBlacklisted = await _tokenBlacklistService.IsTokenBlacklistedAsync(token);
                
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var usernameClaim = User.FindFirst(ClaimTypes.Name);
                var roleClaim = User.FindFirst(ClaimTypes.Role);

                var tokenInfo = new
                {
                    IsValid = !isBlacklisted,
                    IsBlacklisted = isBlacklisted,
                    UserId = userIdClaim?.Value,
                    Username = usernameClaim?.Value,
                    Role = roleClaim?.Value,
                    CheckedAt = DateTime.UtcNow
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Token status retrieved successfully",
                    Data = tokenInfo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while checking token status"
                });
            }
        }
    }
} 