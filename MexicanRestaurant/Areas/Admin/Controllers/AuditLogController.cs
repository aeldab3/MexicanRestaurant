using MexicanRestaurant.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MexicanRestaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Manager")]
    public class AuditLogController : Controller
    {
        private readonly IAuditLogService _auditLogService;
        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }
        public async Task<IActionResult> Index()
        {
            var logs = await _auditLogService.GetAllLogsAsync();
            return View(logs);
        }
    }
}
