using System;

namespace ActiveRoleEngine
{
    /// <inheritdoc />
    /// <summary>
    /// The permission is not defined (in source code)
    /// </summary>
    /// <seealso cref="T:System.Exception" />
    public class PermissionNotDefinedException : Exception
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ActiveRoleEngine.PermissionNotDefinedException" /> class
        /// </summary>
        /// <param name="permissionId">The permission identifier</param>
        public PermissionNotDefinedException(string permissionId) : base($"Permission [{permissionId}] is not defined in the code.")
        {

        }
    }
}
