using System.Collections.Generic;

namespace ActiveRoleEngine
{
    /// <summary>
    /// User permission model
    /// </summary>
    public interface IUserPermission
    {
        /// <summary>
        /// If the user is super admin
        /// </summary>
        bool IsSuperAdmin { get; }

        /// <summary>
        /// If the user is internal user
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is internal user; otherwise, <c>false</c>
        /// </value>
        bool IsInternalUser { get; }

        /// <summary>
        /// If the user is external user
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is external user; otherwise, <c>false</c>
        /// </value>
        bool IsExternalUser { get; }

        /// <summary>
        /// Get or set the permissions granted to the user
        /// </summary>
        List<string> Permissions { get; }
    }
}
