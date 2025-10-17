using System;
using System.ComponentModel.DataAnnotations;

namespace Neko_api.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Userid { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public DateTime Expires { get; set; }

        public bool IsRevoked { get; set; } = false;

        // Optional: navigation property
        // public User User { get; set; }
    }
}
