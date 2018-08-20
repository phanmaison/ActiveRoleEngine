using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;

namespace ActiveRoleEngine
{
    /// <summary>
    /// Engine for working with Role and Permission
    /// <para>Permissions are defined in controller / action with ActivePermissionAttribute</para>
    /// <para>Non</para>
    /// </summary>
    public static class ActiveUserEngine
    {
        #region Const

        /// <summary>
        /// The session key of current user
        /// </summary>
        private const string SESSIONKEY_CURRENTUSER = "ActiveUserSession";

        /// <summary>
        /// The claim username
        /// </summary>
        public const string CLAIM_USERNAME = "ActiveUsername";

        #endregion Const

        #region CurrentUser

        /// <summary>
        /// Gets or sets the current user
        /// <para>If user is authenticated and session data is loss (e.g. timeout), CurrentUser will be restored</para>
        /// </summary>
        /// <value>
        /// The current user.
        /// </value>
        public static IUserAccount CurrentUser
        {
            get
            {
                IUserAccount _user = (IUserAccount)HttpContext.Current.Session[SESSIONKEY_CURRENTUSER];

                // if IsAuthenticated but _user is null (session is cleared)
                // => try to restore user information from database
                // by calling the ActiveRoleEngine.RestoreUserSession which is set in the config
                if (_user == null && IsAuthenticated && ActiveRoleEngine.RestoreUserSessionHandler != null)
                {
                    // try to restore the user
                    // if success (user exists and user data is set to session storage) => return the user
                    if (ActiveRoleEngine.RestoreUserSessionHandler(ClaimUserName))
                        _user = (IUserAccount)HttpContext.Current.Session[SESSIONKEY_CURRENTUSER];
                }

                return _user;
            }
            set => HttpContext.Current.Session[SESSIONKEY_CURRENTUSER] = value;
        }

        #endregion CurrentUser

        #region Properties

        #region CurrentUserClaimIdentity

        /// <summary>
        /// Gets the current user claim identity.
        /// </summary>
        /// <value>
        /// The current user claim identity.
        /// </value>
        private static ClaimsIdentity CurrentUserClaimIdentity => HttpContext.Current.User.Identity as ClaimsIdentity;

        #endregion CurrentUserClaimIdentity

        #region IsAuthenticated

        /// <summary>
        /// Check if the user has logged in or not
        /// </summary>
        public static bool IsAuthenticated => HttpContext.Current?.User?.Identity?.IsAuthenticated ?? false;

        #endregion IsAuthenticated

        #region UserName

        /// <summary>
        /// Get the name of current logged-in user
        /// </summary>
        /// <returns></returns>
        public static string ClaimUserName => CurrentUserClaimIdentity?.FindFirst(CLAIM_USERNAME)?.Value;

        #endregion UserName

        #region UserID

        /// <summary>
        /// Get current UserId, null if the user is not logged in
        /// </summary>
        /// <value>
        /// The user ID.
        /// </value>
        public static object UserId => CurrentUser?.UserId;

        #endregion UserID

        #endregion Properties

        #region Public Methods

        #region LoginUser

        /// <summary>
        /// Set the User as logged-in
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="user">The user.</param>
        /// <param name="rememberMe">if set to <c>true</c> [_remember me].</param>
        public static void LoginUser(HttpRequestBase request, IUserAccount user, bool rememberMe)
        {
            // use Claim
            // https://dotnetcodr.com/2013/02/11/introduction-to-claims-based-security-in-net4-5-with-c-part-1/
            
            // Only store the username / id in the claim
            // the claim will be sent to client in cookie
            // Other user information should be stored in session
            // If the session expired, we can use the claim to get back the user object
            List<Claim> userClaims = new List<Claim>
            {
                    new Claim(CLAIM_USERNAME, user.UserName)
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(userClaims, DefaultAuthenticationTypes.ApplicationCookie);

            IOwinContext owinContext = request.GetOwinContext();
            IAuthenticationManager authenticationManager = owinContext.Authentication;

            DateTime expiryDate = rememberMe ? DateTime.Now.AddYears(1) : DateTime.Now.AddMinutes(30);

            authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = true, ExpiresUtc = expiryDate }, claimsIdentity);

            // Store the user object in current session
            ActiveUserEngine.CurrentUser = user;
        }

        #endregion LoginUser

        #region LogoutUser

        /// <summary>
        /// Log the user out and clear related session and cookies data
        /// </summary>
        /// <param name="request">The request.</param>
        public static void LogoutUser(HttpRequestBase request)
        {
            ActiveUserEngine.CurrentUser = null;
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();

            IOwinContext owinContext = request.GetOwinContext();
            IAuthenticationManager authenticationManager = owinContext.Authentication;
            authenticationManager.SignOut();
        }

        /// <summary>
        /// Log the user out and clear related session and cookies data
        /// </summary>
        /// <param name="request">The request.</param>
        public static void LogoutUser(HttpRequest request)
        {
            LogoutUser(request.RequestContext.HttpContext.Request);
        }

        #endregion LogoutUser

        #endregion Public Methods

        #region HasPermission

        /// <summary>
        /// Check if a specific user has permission
        /// </summary>
        /// <param name="permission">The permission</param>
        /// <param name="user">The user</param>
        /// <returns>
        ///   <c>true</c> if the specified permission has permission; otherwise, <c>false</c>
        /// </returns>
        public static bool HasPermission(PermissionModel permission, IUserAccount user)
        {
            if (permission == null)
                return true;

            if (user == null)
                return false;

            if (user.IsSuperAdmin)
                return true;

            switch (permission.PermissionType)
            {
                case PermissionType.Authenticated:
                    // user is not null => Authenticated
                    return true;
                case PermissionType.SuperAdmin:
                    return user.IsSuperAdmin;
                //case PermissionType.Permisson:
                default:
                    if (user.Permissions == null || !user.Permissions.Contains(permission.PermissionId))
                        return false;

                    return true;
            }
        }

        /// <summary>
        /// Check if CurrentUser has the permission
        /// </summary>
        /// <param name="permission">The permission</param>
        /// <returns>
        ///   <c>true</c> if the specified permission has permission; otherwise, <c>false</c>
        /// </returns>
        public static bool HasPermission(PermissionModel permission)
        {
            return HasPermission(permission, CurrentUser);
        }

        /// <summary>
        /// Check if current user is authorized for permissionId
        /// </summary>
        /// <param name="permissionId">The permission identifier</param>
        /// <returns>
        ///   <c>true</c> if the specified permission identifier has permission; otherwise, <c>false</c>
        /// </returns>
        public static bool HasPermission(string permissionId)
        {
            if (permissionId.IsNullOrEmpty())
                return true;

            PermissionModel permission = ActiveRoleEngine.GetPermissionById(permissionId);

            return HasPermission(permission);
        }

        #endregion HasPermission
    }
}
