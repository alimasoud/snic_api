using System.ComponentModel.DataAnnotations;

namespace snic_api.Models
{
    public class Feature
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Detail { get; set; } = string.Empty;

        [Required]
        public int ProductId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Product Product { get; set; } = null!;
    }
}
