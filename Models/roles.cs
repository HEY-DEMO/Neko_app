using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Neko_api.Models
{
    public class Roles
    {
        [Key]
        public int roleid { get; set; }

        [Required, MaxLength(50)]
        public string rolename { get; set; }

        public ICollection<User_roles> UserRoles { get; set; } = new List<User_roles>();
    }
}
