using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.EntityFrameworkCore;

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

        public async Task<AuditLogViewModel> GetPagedLogsAsync(PaginationInfo pagination)
        {
            var options = new QueryOptions<AuditLog>
            {
                PageNumber = pagination.CurrentPage,
                PageSize = pagination.PageSize,
                OrderByWithFunc = q => q.OrderByDescending(t => t.Timestamp)
            };

            var totalLogs = await _auditRepo.Table.CountAsync();
            var logs = await _auditRepo.GetAllAsync(options);

            return new AuditLogViewModel
            {
                Logs = logs.ToList(),
                Pagination = new PaginationInfo
                {
                    CurrentPage = pagination.CurrentPage,
                    PageSize = pagination.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalLogs / pagination.PageSize)
                }
            };

        }
    }
}
