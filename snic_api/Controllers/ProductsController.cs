using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snic_api.Models;

namespace snic_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] 
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductResponse>>>> GetProducts()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.CreatedByUser)
                    .Include(p => p.Features)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var productResponses = products.Select(p => new ProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    IsActive = p.IsActive,
                    CreatedByUserId = p.CreatedByUserId,
                    CreatedByUsername = p.CreatedByUser.Username,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Features = p.Features.Select(f => new FeatureResponse
                    {
                        Id = f.Id,
                        Title = f.Title,
                        Detail = f.Detail,
                        ProductId = f.ProductId,
                        CreatedAt = f.CreatedAt
                    }).ToList()
                }).ToList();

                return Ok(new ApiResponse<IEnumerable<ProductResponse>>
                {
                    Success = true,
                    Message = "Products retrieved successfully",
                    Data = productResponses
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<IEnumerable<ProductResponse>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving products"
                });
            }
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.CreatedByUser)
                    .Include(p => p.Features)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return NotFound(new ApiResponse<ProductResponse>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                var productResponse = new ProductResponse
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    IsActive = product.IsActive,
                    CreatedByUserId = product.CreatedByUserId,
                    CreatedByUsername = product.CreatedByUser.Username,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    Features = product.Features.Select(f => new FeatureResponse
                    {
                        Id = f.Id,
                        Title = f.Title,
                        Detail = f.Detail,
                        ProductId = f.ProductId,
                        CreatedAt = f.CreatedAt
                    }).ToList()
                };

                return Ok(new ApiResponse<ProductResponse>
                {
                    Success = true,
                    Message = "Product retrieved successfully",
                    Data = productResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ProductResponse>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the product"
                });
            }
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> CreateProduct(CreateProductRequest request)
        {
            try
            {
                // Verify that the creator user exists
                var creatorExists = await _context.Users.AnyAsync(u => u.Id == request.CreatedByUserId);
                if (!creatorExists)
                {
                    return BadRequest(new ApiResponse<ProductResponse>
                    {
                        Success = false,
                        Message = "Creator user not found"
                    });
                }

                var product = new Product
                {
                    Name = request.Name,
                    Price = request.Price,
                    IsActive = request.IsActive,
                    CreatedByUserId = request.CreatedByUserId,
                    CreatedAt = DateTime.UtcNow
                };

                // Add features if provided
                if (request.Features.Any())
                {
                    foreach (var featureRequest in request.Features)
                    {
                        product.Features.Add(new Feature
                        {
                            Title = featureRequest.Title,
                            Detail = featureRequest.Detail,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Reload the product with related data
                var createdProduct = await _context.Products
                    .Include(p => p.CreatedByUser)
                    .Include(p => p.Features)
                    .FirstAsync(p => p.Id == product.Id);

                var productResponse = new ProductResponse
                {
                    Id = createdProduct.Id,
                    Name = createdProduct.Name,
                    Price = createdProduct.Price,
                    IsActive = createdProduct.IsActive,
                    CreatedByUserId = createdProduct.CreatedByUserId,
                    CreatedByUsername = createdProduct.CreatedByUser.Username,
                    CreatedAt = createdProduct.CreatedAt,
                    UpdatedAt = createdProduct.UpdatedAt,
                    Features = createdProduct.Features.Select(f => new FeatureResponse
                    {
                        Id = f.Id,
                        Title = f.Title,
                        Detail = f.Detail,
                        ProductId = f.ProductId,
                        CreatedAt = f.CreatedAt
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new ApiResponse<ProductResponse>
                {
                    Success = true,
                    Message = "Product created successfully",
                    Data = productResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ProductResponse>
                {
                    Success = false,
                    Message = "An error occurred while creating the product"
                });
            }
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> UpdateProduct(int id, UpdateProductRequest request)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.CreatedByUser)
                    .Include(p => p.Features)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return NotFound(new ApiResponse<ProductResponse>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                product.Name = request.Name;
                product.Price = request.Price;
                product.IsActive = request.IsActive;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var productResponse = new ProductResponse
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    IsActive = product.IsActive,
                    CreatedByUserId = product.CreatedByUserId,
                    CreatedByUsername = product.CreatedByUser.Username,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    Features = product.Features.Select(f => new FeatureResponse
                    {
                        Id = f.Id,
                        Title = f.Title,
                        Detail = f.Detail,
                        ProductId = f.ProductId,
                        CreatedAt = f.CreatedAt
                    }).ToList()
                };

                return Ok(new ApiResponse<ProductResponse>
                {
                    Success = true,
                    Message = "Product updated successfully",
                    Data = productResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ProductResponse>
                {
                    Success = false,
                    Message = "An error occurred while updating the product"
                });
            }
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only admins can delete products
        public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                // Check if there are any policies associated with this product
                var hasPolicies = await _context.Policies.AnyAsync(p => p.ProductId == id);
                if (hasPolicies)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Cannot delete product that has associated policies"
                    });
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Product deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the product"
                });
            }
        }
    }
}
