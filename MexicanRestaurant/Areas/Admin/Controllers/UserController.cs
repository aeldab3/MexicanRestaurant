using MexicanRestaurant.Areas.Admin.Models;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MexicanRestaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Manager")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditLogHelper _auditLogHelper;
        private readonly ILogger<UserController> _logger;
        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IAuditLogHelper auditLogHelper, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _auditLogHelper = auditLogHelper;
        }

        public async Task<IActionResult> Index()
        {
            List<ApplicationUser> users = new List<ApplicationUser>();
            foreach (ApplicationUser user in _userManager.Users)
            {
                user.RoleNames = await _userManager.GetRolesAsync(user);
                users.Add(user);
            }

            UserViewModel model = new UserViewModel
            {
                Users = users,
                Roles = _roleManager.Roles
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound("Can't find this User");

                IdentityRole adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole != null && await _userManager.IsInRoleAsync(user, adminRole.Name))
                {
                    TempData["Error"] = "Cannot delete an Admin user.";
                    return RedirectToAction("Index");
                }

                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    await _auditLogHelper.LogActionAsync(HttpContext, "Delete", "ApplicationUser", user.Id, $"User {user.UserName} deleted by {User.Identity.Name}");
                    TempData["Success"] = "User deleted successfully.";
                }
                else
                    TempData["Error"] = "Failed to delete user.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID {id}.");
                TempData["Error"] = "An error occurred while deleting the user.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToRole(string userId, string roleName)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(userId);
                if (user == null) return NotFound("Can't find this User");

                IdentityRole role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) return NotFound("Can't find this Role");
                
                IdentityResult result = await _userManager.AddToRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    await _auditLogHelper.LogActionAsync(HttpContext, "AddToRole", "ApplicationUser", user.Id, $"User {user.UserName} added to role {roleName} by {User.Identity.Name}");
                    TempData["Success"] = $"User {user.UserName} added to role {roleName} successfully.";
                }
                else
                    TempData["Error"] = "Failed to add user to role.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding user with ID {userId} to role {roleName}.");
                TempData["Error"] = "An error occurred while adding the user to the role.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromRole(string userId, string roleName)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(userId);
                if (user == null) return NotFound("Can't find this User");

                IdentityRole role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) return NotFound("Can't find this Role");

                if (role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) && await _userManager.IsInRoleAsync(user, roleName))
                {
                    TempData["Error"] = "Cannot remove user from the Admin role.";
                    return RedirectToAction("Index");
                }

                IdentityResult result = await _userManager.RemoveFromRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    await _auditLogHelper.LogActionAsync(HttpContext, "RemoveFromRole", "ApplicationUser", user.Id, $"User {user.UserName} removed from role {roleName} by {User.Identity.Name}");
                    TempData["Success"] = $"User {user.UserName} removed from role {roleName} successfully.";
                }
                else
                    TempData["Error"] = "Failed to remove user from role.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing user with ID {userId} from role {roleName}.");
                TempData["Error"] = "An error occurred while removing the user from the role.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                    TempData["Error"] = "Role name cannot be empty.";

                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    TempData["Error"] = "Role already exists.";
                    return RedirectToAction("Index");
                }

                IdentityRole role = new IdentityRole(roleName);

                IdentityResult result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    await _auditLogHelper.LogActionAsync(HttpContext, "CreateRole", "IdentityRole", role.Id, $"Role {roleName} created by {User.Identity.Name}");
                    TempData["Success"] = $"Role {roleName} created successfully.";
                }
                else
                    TempData["Error"] = "Failed to create role.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating role {roleName}.");
                TempData["Error"] = "An error occurred while creating the role.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                    TempData["Error"] = "Role name cannot be empty.";

                IdentityRole role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    TempData["Error"] = "Role not found.";
                    return RedirectToAction("Index");
                }
                if (role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = "Cannot delete the Admin role.";
                    return RedirectToAction("Index");
                }

                IdentityResult result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    await _auditLogHelper.LogActionAsync(HttpContext, "DeleteRole", "IdentityRole", role.Id, $"Role {roleName} deleted by {User.Identity.Name}");
                    TempData["Success"] = $"Role {roleName} deleted successfully.";
                }
                else
                    TempData["Error"] = "Failed to delete role.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting role {roleName}.");
                TempData["Error"] = "An error occurred while deleting the role.";
            }
            return RedirectToAction("Index");
        }
    }
}
