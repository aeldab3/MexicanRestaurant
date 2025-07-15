using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;

namespace MexicanRestaurant.WebUI.ViewModels
{
    public class AuditLogViewModel
    {
        public List<AuditLog> Logs { get; set; } = new();
        public PaginationInfo Pagination { get; set; }
    }
}
