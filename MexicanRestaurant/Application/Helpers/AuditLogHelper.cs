using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace MexicanRestaurant.Application.Helpers
{
    public class AuditLogHelper : IAuditLogHelper
    {
        private readonly IAuditLogService _auditLogService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuditLogHelper> _logger;
        public AuditLogHelper(IAuditLogService auditLogService, UserManager<ApplicationUser> userManager, ILogger<AuditLogHelper> logger)
        {
            _auditLogService = auditLogService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task LogActionAsync(HttpContext context, string action, string entity, string entityId, string details)
        {
            try
            {
                if (context == null || context.User == null)
                {
                    _logger.LogWarning("HttpContext or User is null. Cannot log audit action.");
                    return;
                }
                var user = await _userManager.GetUserAsync(context.User);
                if (user == null)
                {
                    _logger.LogWarning("User not found during audit logging.");
                    return;
                }
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Unknown";
                await _auditLogService.LogAsync(
                    userId: user.Id,
                    email: user.Email,
                    role: role,
                    action: action,
                    entity: entity,
                    entityId: entityId,
                    details: details
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit action: {Action}", action);
            }
        }
    }
}
