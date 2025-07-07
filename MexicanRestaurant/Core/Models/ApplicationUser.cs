using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace MexicanRestaurant.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        [NotMapped]
        public IList<string> RoleNames { get; set; } = new List<string>();
    }
}
