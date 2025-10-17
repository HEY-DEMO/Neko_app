using System.ComponentModel.DataAnnotations.Schema;

namespace Neko_api.Models
{
    public class User_roles
    {
        public int userid { get; set; }
        public int roleid { get; set; }

        public User User { get; set; }
        public Roles Role { get; set; }
    }
}
