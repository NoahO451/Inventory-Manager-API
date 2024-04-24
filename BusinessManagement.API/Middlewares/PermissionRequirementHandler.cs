using App.Models;
using App.Models.DTO;
using App.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace App.Middlewares
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionName { get; }

        public PermissionRequirement(string permissionName)
        {
            PermissionName = permissionName;
        }
    }

    public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>
    {
        private IUserRolePermissionRepository _rolePermissionRepository;
        private ILogger<UserRepository> _logger;

        public PermissionRequirementHandler(IUserRolePermissionRepository rolePermissionRepository, DataContext context, ILogger<UserRepository> logger)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _logger = logger;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            try
            {
                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    string? authId = context.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                    authId = "";

                    if (string.IsNullOrWhiteSpace(authId))
                    {
                        _logger.LogWarning("{trace} Failed to get user permissions", LogHelper.TraceLog());
                        return;
                    }

                    UserRolePermission permissions = await _rolePermissionRepository.GetUserRolePermissions(authId);

                    if (permissions != null && permissions.Permissions.Count > 0)
                    {
                        if (permissions != null && permissions.Permissions.Contains(requirement.PermissionName))
                        {
                            context.Succeed(requirement);
                        }
                    } 
                    else
                    {
                        _logger.LogWarning("{trace} Failed to get user permissions", LogHelper.TraceLog());
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception. Failed to confirm user permission", LogHelper.TraceLog());
                return;
            }
            finally
            {
                await Task.CompletedTask;
            }
        }
    }
}
