using System.ComponentModel.DataAnnotations;

namespace snic_api.Models
{
    public class BlacklistedToken
    {
        public int Id { get; set; }
        
        [Required]
        public string TokenId { get; set; } = string.Empty; // JWT ID (jti claim)
        
        [Required]
        public string Token { get; set; } = string.Empty; // Full token for reference
        
        public int UserId { get; set; }
        
        public DateTime BlacklistedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime ExpiresAt { get; set; } // When the token would naturally expire
        
        public string Reason { get; set; } = "Logout"; // Reason for blacklisting
    }
} 