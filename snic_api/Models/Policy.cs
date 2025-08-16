using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace snic_api.Models
{
    public class Policy
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string HolderName { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Premium { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Product Product { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
