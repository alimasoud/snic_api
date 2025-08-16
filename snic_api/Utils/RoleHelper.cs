using snic_api.Models;

namespace snic_api.Utils
{
    public static class RoleHelper
    {
        /// <summary>
        /// Converts UserRole enum to string representation
        /// </summary>
        public static string GetRoleString(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "Admin",
                UserRole.Customer => "Customer",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Converts string to UserRole enum
        /// </summary>
        public static UserRole ParseRole(string roleString)
        {
            return roleString?.ToLower() switch
            {
                "admin" => UserRole.Admin,
                "customer" => UserRole.Customer,
                "0" => UserRole.Customer,
                "1" => UserRole.Admin,
                _ => UserRole.Customer // Default to Customer
            };
        }

        /// <summary>
        /// Checks if a role has admin privileges
        /// </summary>
        public static bool IsAdmin(UserRole role)
        {
            return role == UserRole.Admin;
        }

        /// <summary>
        /// Checks if a role has customer privileges
        /// </summary>
        public static bool IsCustomer(UserRole role)
        {
            return role == UserRole.Customer;
        }

        /// <summary>
        /// Gets all available roles
        /// </summary>
        public static IEnumerable<UserRole> GetAllRoles()
        {
            return Enum.GetValues<UserRole>();
        }

        /// <summary>
        /// Gets role description for display purposes
        /// </summary>
        public static string GetRoleDescription(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "Administrator with full system access",
                UserRole.Customer => "Customer with standard user privileges",
                _ => "Unknown role"
            };
        }
    }
}
