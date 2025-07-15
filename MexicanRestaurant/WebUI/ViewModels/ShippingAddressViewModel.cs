using System.ComponentModel.DataAnnotations;

namespace MexicanRestaurant.WebUI.ViewModels
{
    public class ShippingAddressViewModel
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; }

        [Required, StringLength(50)]
        public string LastName { get; set; }

        [Required, StringLength(50)]
        public string Street { get; set; }

        [Required, StringLength(50)]
        public string City { get; set; }

        [Required, StringLength(50)]
        public string State { get; set; }

        [Required, StringLength(10)]
        public string Zipcode { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }
    }
}
