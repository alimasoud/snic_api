using System.ComponentModel.DataAnnotations;

namespace snic_api.Models
{
    public class CreateProductRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public int CreatedByUserId { get; set; }

        public List<CreateFeatureRequest> Features { get; set; } = new List<CreateFeatureRequest>();
    }

    public class UpdateProductRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; }
    }

    public class CreateFeatureRequest
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Detail { get; set; } = string.Empty;
    }

    public class UpdateFeatureRequest
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Detail { get; set; } = string.Empty;
    }

    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<FeatureResponse> Features { get; set; } = new List<FeatureResponse>();
    }

    public class FeatureResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
