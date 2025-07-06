using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace MexicanRestaurant.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        [NotMapped]
        public IList<string> RoleNames { get; set; } = new List<string>();
    }
}
