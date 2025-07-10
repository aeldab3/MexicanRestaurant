using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;

namespace MexicanRestaurant.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IRepository<AuditLog> _auditRepo;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(IRepository<AuditLog> auditRepo, ILogger<AuditLogService> logger)
        {
            _auditRepo = auditRepo;
            _logger = logger;
        }

        public async Task LogAsync(string userId, string email, string role, string action, string entity, string entityId, string details)
        {
            try
            {
                var log = new AuditLog
                {
                    UserId = userId,
                    Email = email,
                    Role = role,
                    ActionType = action,
                    EntityName = entity,
                    EntityId = entityId,
                    Details = details,
                    Timestamp = DateTime.UtcNow
                };
                await _auditRepo.AddAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit action: {Action} for entity: {Entity} with ID: {EntityId}", action, entity, entityId);
                return;
            }
        }

        public async Task<IEnumerable<AuditLog>> GetAllLogsAsync()
        {
            return await _auditRepo.GetAllAsync();
        }
    }
}
