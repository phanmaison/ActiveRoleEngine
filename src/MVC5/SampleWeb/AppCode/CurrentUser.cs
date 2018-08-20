using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ActiveRoleEngine;
using SampleWeb.DbContext;
using SampleWeb.Domain;

namespace SampleWeb
{

    /// <summary>
    /// Current User
    /// </summary>
    public static partial class CurrentUser
    {
        /// <summary>
        /// If the user is logged in or not
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is authenticated; otherwise, <c>false</c>.
        /// </value>
        public static bool IsAuthenticated => ActiveUserEngine.IsAuthenticated;

        public static SysUser User
        {
            get => (SysUser)ActiveUserEngine.CurrentUser;
        }

        #region RestoreUserSession

        /// <summary>
        /// Restores the user session if session timeout
        /// <para>Triggered by the RoleEngine</para>
        /// </summary>
        /// <param name="claimUserName">Name of the claim user.</param>
        /// <returns></returns>
        public static bool RestoreUserSession(string claimUserName)
        {
            // re-create session
            SysUser user = SampleDbContext.Current.Users.FirstOrDefault(u => u.Username == claimUserName);

            if (user == null)
            {
                // if the user is not found in database
                ActiveUserEngine.LogoutUser(HttpContext.Current.Request);
                return false;
            }
            else
            {
                // get the user permission
                user.Permissions = GetUserPermission(user.UserId);
                    //SampleDbContext.Current.UserPermissions
                    //.Where(x => x.UserId == user.UserId)
                    //.Select(up => up.PermissionId).ToList();

                ActiveUserEngine.CurrentUser = user;

                return true;
            }
        }

        #endregion RestoreUserSession

        public static List<string> GetUserPermission(Guid userId)
        {
            // get inherit permission from roles

            // get all assigned roles
            var userRoles = SampleDbContext.Current.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => SampleDbContext.Current.Roles.FirstOrDefault(r => r.RoleId == ur.RoleId).RoleId).ToList();

            // get all role permissions
            var rolePermissions = SampleDbContext.Current.RolePermissions
                .Where(rp => userRoles.Contains(rp.RoleId))
                .Select(rp => rp.PermissionId).ToList();

            // remove override permission
            var removePermissions = SampleDbContext.Current.UserPermissions
                .Where(up => up.UserId == userId && up.IsAdd == false)
                .Select(up => up.PermissionId).ToList();

            rolePermissions.RemoveAll(p => removePermissions.Contains(p));

            // add override permission
            var addPermission = SampleDbContext.Current.UserPermissions
                .Where(up => up.UserId == userId && up.IsAdd == true)
                .Select(up => up.PermissionId).ToList();

            rolePermissions.AddRange(addPermission);

            // return
            return rolePermissions.Distinct().ToList();
        }
    }
}
