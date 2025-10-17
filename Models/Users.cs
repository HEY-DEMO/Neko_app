using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Neko_api.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Userid { get; set; }

        [Required, StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(255, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required, Phone, StringLength(10, MinimumLength = 10)]
        public string Mobile { get; set; } = string.Empty;

        public ICollection<User_roles> UserRoles { get; set; } = new List<User_roles>();
    }
}
