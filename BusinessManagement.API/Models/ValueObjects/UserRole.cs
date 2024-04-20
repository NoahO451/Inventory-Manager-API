using App.Models.Enumerations;

namespace App.Models.ValueObjects
{
    public record UserRole
    {
        private UserRole() { }
        public UserRole(List<Role> userRoles)
        {
            //if (userRoles.Count < 1)
            //    throw new ArgumentException("User must have at least one role", nameof(userRoles));

            //if (userRoles.Contains(Role.Owner) && userRoles.Contains(Role.SubUser))
            //    throw new ArgumentException("Cannot be an owner and sub user at the same time", nameof(userRoles));

            //if (userRoles.Contains(Role.None) && userRoles.Count > 1)
            //    throw new ArgumentException("Cannot have role None and any other role at the same time", nameof(userRoles));

            //UserRoles = userRoles;
        }

        public List<Role> UserRoles { get; private set; }
    }
}
