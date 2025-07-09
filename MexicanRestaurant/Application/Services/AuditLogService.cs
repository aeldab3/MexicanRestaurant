using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Infrastructure.Data;

namespace MexicanRestaurant.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IRepository<AuditLog> _auditRepo;

        public AuditLogService(IRepository<AuditLog> auditRepo)
        {
            _auditRepo = auditRepo;
        }

        public async Task LogAsync(string userId, string email, string role, string action, string entity, string entityId, string details)
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

        public async Task<IEnumerable<AuditLog>> GetAllLogsAsync()
        {
            return await _auditRepo.GetAllAsync();
        }
    }
}
