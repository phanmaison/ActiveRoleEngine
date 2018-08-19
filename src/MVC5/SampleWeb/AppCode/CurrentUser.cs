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
                user.Permissions = SampleDbContext.Current.UserPermissions
                    .Where(x => x.UserId == user.UserId)
                    .Select(up => up.PermissionId).ToList();

                ActiveUserEngine.CurrentUser = user;

                return true;
            }
        }

        #endregion RestoreUserSession
    }
}
