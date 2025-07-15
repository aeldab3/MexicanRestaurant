using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Core.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(string userId,string email, string role, string action, string entity, string entityId, string details);
        Task<AuditLogViewModel> GetPagedLogsAsync(PaginationInfo pagination);
    }
}
