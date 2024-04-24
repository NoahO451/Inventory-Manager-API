using App.Models;
using App.Models.DTO;
using App.Models.ValueObjects;
using Dapper;

namespace App.Repositories
{
    public interface IUserRolePermissionRepository
    {
        Task<UserRolePermission> GetUserRolePermissions(string auth0Id);
    }

    public class UserRolePermissionRepository : IUserRolePermissionRepository
    {
        private DataContext _context;

        private ILogger<UserRepository> _logger;

        public UserRolePermissionRepository(DataContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtains roles and permissions. Takes in auth0Id to find the user.
        /// </summary>
        /// <param name="auth0Id"></param>
        /// <returns>Returns the user's role and a list of permissions </returns>
        public async Task<UserRolePermission> GetUserRolePermissions(string auth0Id)
        {
            using (var connection = _context.CreateConnection())
            {
                // Get the actual primary key for the user. We don't want to join off of authIds all the time. 
                string sqlUserId = """
                SELECT 
                    user_id
                FROM 
                    user_data
                WHERE auth0_id = @Auth0Id;
                """;

                int userId = await connection.QuerySingleOrDefaultAsync<int>(sqlUserId, new { Auth0Id = auth0Id });

                string sqlRolesAndPermissions = """
                SELECT 
                    r.role_name,
                    p.permission_name
                FROM 
                    user_role ur
                    LEFT JOIN role r ON ur.role_id = r.role_id
                    LEFT JOIN role_permission rp ON r.role_id = rp.role_id
                    LEFT JOIN permission p ON rp.permission_id = p.permission_id
                WHERE 
                    ur.user_id = @UserId;
                """;
                
                var rolePermissions = await connection.QueryAsync<dynamic>(sqlRolesAndPermissions, new { UserId = userId });

                var userRole = new UserRolePermission
                {
                    Roles = new List<string>(),
                    Permissions = new List<string>()
                };

                foreach (var rp in rolePermissions)
                {
                    if (!userRole.Roles.Contains(rp.role_name))
                    {
                        userRole.Roles.Add(rp.role_name);
                    }

                    if (!userRole.Permissions.Contains(rp.permission_name))
                    {
                        userRole.Permissions.Add(rp.permission_name);
                    }
                }

                return userRole;
            }
        }

        private record RolePermissionResult
        {
            public string Role { get; init; }
            public string Permission { get; init; }
        }
    }
}