using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Views.Shared;
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

        public async Task<IActionResult> Index(int page = 1)
        {
            var pagination = new PaginationInfo { CurrentPage = page, PageSize = 15 };
            var logs = await _auditLogService.GetPagedLogsAsync(pagination);
            return View(logs);
        }
    }
}
