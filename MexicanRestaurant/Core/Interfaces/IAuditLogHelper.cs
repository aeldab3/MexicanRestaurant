namespace MexicanRestaurant.Core.Interfaces
{
    public interface IAuditLogHelper
    {
        Task LogActionAsync(HttpContext context, string action, string entity, string entityId, string details);
    }
}