using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snic_api.Models;

namespace snic_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class PoliciesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PoliciesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/policies
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PolicyResponse>>>> GetPolicies()
        {
            try
            {
                var policies = await _context.Policies
                    .Include(p => p.Product)
                    .Include(p => p.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var policyResponses = policies.Select(p => new PolicyResponse
                {
                    Id = p.Id,
                    PolicyNumber = p.PolicyNumber,
                    HolderName = p.HolderName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Premium = p.Premium,
                    ProductId = p.ProductId,
                    ProductName = p.Product.Name,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                return Ok(new ApiResponse<IEnumerable<PolicyResponse>>
                {
                    Success = true,
                    Message = "Policies retrieved successfully",
                    Data = policyResponses
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<IEnumerable<PolicyResponse>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving policies"
                });
            }
        }

        // GET: api/policies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PolicyResponse>>> GetPolicy(int id)
        {
            try
            {
                var policy = await _context.Policies
                    .Include(p => p.Product)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (policy == null)
                {
                    return NotFound(new ApiResponse<PolicyResponse>
                    {
                        Success = false,
                        Message = "Policy not found"
                    });
                }

                var policyResponse = new PolicyResponse
                {
                    Id = policy.Id,
                    PolicyNumber = policy.PolicyNumber,
                    HolderName = policy.HolderName,
                    StartDate = policy.StartDate,
                    EndDate = policy.EndDate,
                    Premium = policy.Premium,
                    ProductId = policy.ProductId,
                    ProductName = policy.Product.Name,
                    UserId = policy.UserId,
                    Username = policy.User.Username,
                    CreatedAt = policy.CreatedAt,
                    UpdatedAt = policy.UpdatedAt
                };

                return Ok(new ApiResponse<PolicyResponse>
                {
                    Success = true,
                    Message = "Policy retrieved successfully",
                    Data = policyResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PolicyResponse>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the policy"
                });
            }
        }

        // GET: api/policies/by-product/5
        [HttpGet("by-product/{productId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PolicyResponse>>>> GetPoliciesByProduct(int productId)
        {
            try
            {
                var policies = await _context.Policies
                    .Include(p => p.Product)
                    .Include(p => p.User)
                    .Where(p => p.ProductId == productId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var policyResponses = policies.Select(p => new PolicyResponse
                {
                    Id = p.Id,
                    PolicyNumber = p.PolicyNumber,
                    HolderName = p.HolderName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Premium = p.Premium,
                    ProductId = p.ProductId,
                    ProductName = p.Product.Name,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                return Ok(new ApiResponse<IEnumerable<PolicyResponse>>
                {
                    Success = true,
                    Message = "Policies retrieved successfully",
                    Data = policyResponses
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<IEnumerable<PolicyResponse>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving policies"
                });
            }
        }

        // GET: api/policies/by-user/5
        [HttpGet("by-user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PolicyResponse>>>> GetPoliciesByUser(int userId)
        {
            try
            {
                // Verify that the user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    return NotFound(new ApiResponse<IEnumerable<PolicyResponse>>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                var policies = await _context.Policies
                    .Include(p => p.Product)
                    .Include(p => p.User)
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var policyResponses = policies.Select(p => new PolicyResponse
                {
                    Id = p.Id,
                    PolicyNumber = p.PolicyNumber,
                    HolderName = p.HolderName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Premium = p.Premium,
                    ProductId = p.ProductId,
                    ProductName = p.Product.Name,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                return Ok(new ApiResponse<IEnumerable<PolicyResponse>>
                {
                    Success = true,
                    Message = $"Policies for user retrieved successfully ({policyResponses.Count} found)",
                    Data = policyResponses
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<IEnumerable<PolicyResponse>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user policies"
                });
            }
        }

        // POST: api/policies
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PolicyResponse>>> CreatePolicy(CreatePolicyRequest request)
        {
            try
            {
                // Verify that the product exists
                var productExists = await _context.Products.AnyAsync(p => p.Id == request.ProductId);
                if (!productExists)
                {
                    return BadRequest(new ApiResponse<PolicyResponse>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                // Verify that the user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
                if (!userExists)
                {
                    return BadRequest(new ApiResponse<PolicyResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                // Check if policy number already exists
                var policyNumberExists = await _context.Policies.AnyAsync(p => p.PolicyNumber == request.PolicyNumber);
                if (policyNumberExists)
                {
                    return BadRequest(new ApiResponse<PolicyResponse>
                    {
                        Success = false,
                        Message = "Policy number already exists"
                    });
                }

                var policy = new Policy
                {
                    PolicyNumber = request.PolicyNumber,
                    HolderName = request.HolderName,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Premium = request.Premium,
                    ProductId = request.ProductId,
                    UserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Policies.Add(policy);
                await _context.SaveChangesAsync();

                // Reload the policy with related data
                var createdPolicy = await _context.Policies
                    .Include(p => p.Product)
                    .Include(p => p.User)
                    .FirstAsync(p => p.Id == policy.Id);

                var policyResponse = new PolicyResponse
                {
                    Id = createdPolicy.Id,
                    PolicyNumber = createdPolicy.PolicyNumber,
                    HolderName = createdPolicy.HolderName,
                    StartDate = createdPolicy.StartDate,
                    EndDate = createdPolicy.EndDate,
                    Premium = createdPolicy.Premium,
                    ProductId = createdPolicy.ProductId,
                    ProductName = createdPolicy.Product.Name,
                    UserId = createdPolicy.UserId,
                    Username = createdPolicy.User.Username,
                    CreatedAt = createdPolicy.CreatedAt,
                    UpdatedAt = createdPolicy.UpdatedAt
                };

                return CreatedAtAction(nameof(GetPolicy), new { id = policy.Id }, new ApiResponse<PolicyResponse>
                {
                    Success = true,
                    Message = "Policy created successfully",
                    Data = policyResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PolicyResponse>
                {
                    Success = false,
                    Message = "An error occurred while creating the policy"
                });
            }
        }

        // PUT: api/policies/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PolicyResponse>>> UpdatePolicy(int id, UpdatePolicyRequest request)
        {
            try
            {
                var policy = await _context.Policies
                    .Include(p => p.Product)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (policy == null)
                {
                    return NotFound(new ApiResponse<PolicyResponse>
                    {
                        Success = false,
                        Message = "Policy not found"
                    });
                }

                // Verify that the product exists
                var productExists = await _context.Products.AnyAsync(p => p.Id == request.ProductId);
                if (!productExists)
                {
                    return BadRequest(new ApiResponse<PolicyResponse>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                // Verify that the user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
                if (!userExists)
                {
                    return BadRequest(new ApiResponse<PolicyResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                policy.HolderName = request.HolderName;
                policy.StartDate = request.StartDate;
                policy.EndDate = request.EndDate;
                policy.Premium = request.Premium;
                policy.ProductId = request.ProductId;
                policy.UserId = request.UserId;
                policy.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Reload with updated product and user information
                await _context.Entry(policy).Reference(p => p.Product).LoadAsync();
                await _context.Entry(policy).Reference(p => p.User).LoadAsync();

                var policyResponse = new PolicyResponse
                {
                    Id = policy.Id,
                    PolicyNumber = policy.PolicyNumber,
                    HolderName = policy.HolderName,
                    StartDate = policy.StartDate,
                    EndDate = policy.EndDate,
                    Premium = policy.Premium,
                    ProductId = policy.ProductId,
                    ProductName = policy.Product.Name,
                    UserId = policy.UserId,
                    Username = policy.User.Username,
                    CreatedAt = policy.CreatedAt,
                    UpdatedAt = policy.UpdatedAt
                };

                return Ok(new ApiResponse<PolicyResponse>
                {
                    Success = true,
                    Message = "Policy updated successfully",
                    Data = policyResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PolicyResponse>
                {
                    Success = false,
                    Message = "An error occurred while updating the policy"
                });
            }
        }

        // DELETE: api/policies/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only admins can delete policies
        public async Task<ActionResult<ApiResponse<object>>> DeletePolicy(int id)
        {
            try
            {
                var policy = await _context.Policies.FindAsync(id);

                if (policy == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Policy not found"
                    });
                }

                _context.Policies.Remove(policy);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Policy deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the policy"
                });
            }
        }

        // GET: api/policies/active
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PolicyResponse>>>> GetActivePolicies()
        {
            try
            {
                var currentDate = DateTime.UtcNow;
                var activePolicies = await _context.Policies
                    .Include(p => p.Product)
                    .Include(p => p.User)
                    .Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var policyResponses = activePolicies.Select(p => new PolicyResponse
                {
                    Id = p.Id,
                    PolicyNumber = p.PolicyNumber,
                    HolderName = p.HolderName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Premium = p.Premium,
                    ProductId = p.ProductId,
                    ProductName = p.Product.Name,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                return Ok(new ApiResponse<IEnumerable<PolicyResponse>>
                {
                    Success = true,
                    Message = "Active policies retrieved successfully",
                    Data = policyResponses
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<IEnumerable<PolicyResponse>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving active policies"
                });
            }
        }
    }
}
