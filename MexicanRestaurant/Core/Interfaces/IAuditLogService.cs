using MexicanRestaurant.Core.Models;

namespace MexicanRestaurant.Core.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(string userId,string email, string role, string action, string entity, string entityId, string details);
        Task<IEnumerable<AuditLog>> GetAllLogsAsync();
    }
}
