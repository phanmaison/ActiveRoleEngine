using System;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace ActiveRoleEngine
{
    /*
     * Refer to the AuthorizeAttribute
     * 
     * This ActivePermissionAttribute is re-written instead of inheritance
     * 
     * */

    // TODO: test the cache with authorization

    /// <inheritdoc cref="FilterAttribute" />
    /// <summary>
    /// Decorate the controller or action as well as validate user permission
    /// </summary>
    /// <seealso cref="T:System.Web.Mvc.FilterAttribute" />
    /// <seealso cref="T:System.Web.Mvc.IAuthorizationFilter" />
    /// <seealso cref="T:System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class ActivePermissionAttribute : FilterAttribute, IAuthorizationFilter, IPermissionAttribute
    {
        #region IPermissionInfo

        #region Permission

        /// <inheritdoc />
        /// <summary>
        /// Custom permission id
        /// <para>If provide, it will override the area/controller/action</para>
        /// </summary>
        /// <value>
        /// The permission identifier.
        /// </value>
        public string Permission { get; set; }

        #endregion Permission

        #region Area

        /// <inheritdoc />
        /// <summary>
        /// Area Name if any (default from controller)
        /// </summary>
        /// <value>
        /// The area.
        /// </value>
        public string Area { get; set; }

        #endregion Area

        #region Controller

        /// <inheritdoc />
        /// <summary>
        /// Name of Controller if any (default from Controller)
        /// </summary>
        /// <value>
        /// The controller.
        /// </value>
        public string Controller { get; set; }

        #endregion Controller

        #region Action

        /// <inheritdoc />
        /// <summary>
        /// Name of Action if any (default from Action Name)
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public string Action { get; set; }

        #endregion Action

        #region Group

        /// <inheritdoc />
        /// <summary>
        /// Group of permission for grouping on UI (Action will inherit from Controller if not defined)
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public string Group { get; set; }

        #endregion Group

        #region Description

        /// <inheritdoc />
        /// <summary>
        /// Description of the permission
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        #endregion Description

        #region PermissionType

        /// <inheritdoc />
        /// <summary>
        /// Type of permission
        /// </summary>
        public PermissionType PermissionType { get; set; } = PermissionType.Permisson;

        #endregion PermissionType

        #region PermissionId

        /// <inheritdoc />
        /// <summary>
        /// Unique key of the permission
        /// </summary>
        /// <value>
        /// The unique key identifier.
        /// </value>
        public string PermissionId => ActiveRoleEngineHelper.GetPermissionId(this.Permission, this.Area, this.Controller, this.Action);

        #endregion PermissionId

        #endregion IPermissionInfo

        #region AuthorizingPermissionId

        /// <summary>
        /// The current authorizing permission id
        /// <para>If the authorization failed, may display this permission id to user</para>
        /// </summary>
        /// <value>
        /// The authorizing permission
        /// </value>
        private string AuthorizingPermissionId { get; set; }

        #endregion AuthorizingPermissionId

        #region Methods

        #region AuthorizeUser

        // This method must be thread-safe since it is called by the thread-safe OnCacheAuthorization() method.

        /**
         * Authorization flow:
         * - First time, the OnAuthorization will be invoked
         * + If the authorization is skip, the action process will continue
         * + Else, the cache handler will be added
         * - So in both case (cache and no-cache), the authorization will call AuthorizeCore to validate
         * 
         * */

        /// <summary>
        /// Main point to authorize the user
        /// </summary>
        /// <param name="filterContext">The filter context of the action</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">httpContext</exception>
        /// <exception cref="System.Exception">SystemPermissions is not defined</exception>
        protected virtual AuthorizeResult AuthorizeUser(AuthorizationContext filterContext)
        {
            // Note: use the filterfilterContext so that we may get the controller's attribute
            if (filterContext == null)
                throw new ArgumentNullException(nameof(filterContext));

            HttpContextBase httpContext = filterContext.HttpContext;

            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            IUserPermission user = ActiveUserEngine.CurrentUser;

            // no user data => unauthorized
            if (user == null)
                return AuthorizeResult.FailedNotLoggedIn;

            // TODO: temp disable
            // always authorize system admin
            if (user.IsSuperAdmin)
                return AuthorizeResult.Success;

            // FROM NOW, USER IS NOT SUPER ADMIN

            // TODO:
            // https://www.ryadel.com/en/asp-net-mvc-fix-ambiguous-action-methods-errors-multiple-action-methods-action-name-c-sharp-core/
            // Get the MethodInfo of the action
            // https://stackoverflow.com/questions/1972234/how-do-i-get-a-methodinfo-from-an-actionexecutingcontext

            MethodInfo methodInfo = filterContext.ActionDescriptor.GetMethodInfo();
            Type controllerType = filterContext.ActionDescriptor.ControllerDescriptor.ControllerType;

            PermissionModel permission = null;

            if (ActiveRoleEngine.PermissionDictionaryCache.ContainsKey(methodInfo))
            {
                permission = ActiveRoleEngine.PermissionDictionaryCache[methodInfo];
            }
            else if (ActiveRoleEngine.PermissionDictionaryCache.ContainsKey(controllerType))
            {
                permission = ActiveRoleEngine.PermissionDictionaryCache[controllerType];
            }

            if (permission == null)
            {
                // this should not happen, anyway, throw an exception for debugging

                string actionNamespace = ActiveRoleEngineHelper.GetDebugNamespace(methodInfo);

                throw new SystemException($"FATAL ERROR: cannot found the defined permission for '{actionNamespace}'");
            }

            // to display the permission on error page
            this.AuthorizingPermissionId = permission.PermissionId;

            switch (permission.PermissionType)
            {
                case PermissionType.Authenticated:
                    // ActiveUserEngine.CurrentUser is not null => Authenticated
                    return AuthorizeResult.Success;

                case PermissionType.AuthorizedInternal:
                    // only authorize internal user
                    return user.IsInternalUser ? AuthorizeResult.Success : AuthorizeResult.FailedNotAuthorized;

                case PermissionType.AuthorizedExternal:
                    // only authorize external user
                    return user.IsExternalUser ? AuthorizeResult.Success : AuthorizeResult.FailedNotAuthorized;

                case PermissionType.SuperAdmin:
                    // user must be super admin
                    return user.IsSuperAdmin ? AuthorizeResult.Success : AuthorizeResult.FailedSuperAdminOnly;

                //case PermissionType.Permisson:
                default:
                    // user must be granted permission to access
                    if (user.Permissions == null || !user.Permissions.Contains(permission.PermissionId))
                        return AuthorizeResult.FailedNotAuthorized;

                    return AuthorizeResult.Success;
            }
        }

        #endregion AuthorizeUser

        #region OnAuthorization

        /// <inheritdoc />
        /// <summary>
        /// Called when authorization is required.
        /// </summary>
        /// <param name="filterContext">The filter context</param>
        /// <exception cref="T:System.ArgumentNullException">filterContext</exception>
        /// <exception cref="T:System.InvalidOperationException">ActivePermissionAttribute: Cannot Use Within Child Action Cache</exception>
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            //filterContext.Controller.ControllerContext.

            if (filterContext == null)
                throw new ArgumentNullException(nameof(filterContext));

            if (OutputCacheAttribute.IsChildActionCacheActive(filterContext))
                // If a child action cache block is active, we need to fail immediately, even if authorization
                // would have succeeded. The reason is that there's no way to hook a callback to rerun
                // authorization before the fragment is served from the cache, so we can't guarantee that this
                // filter will be re-run on subsequent requests.

                // MvcResources.AuthorizeAttribute_CannotUseWithinChildActionCache
                throw new InvalidOperationException("ActivePermissionAttribute: Cannot Use Within Child Action Cache");

            #region Skip Authorization

            /*
             * Skip the authorization if
             * - Controller / Action has AllowAnonymousAttribute
             * - Action is NonAction
             * - Action is child action
             * 
             * */

            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true) ||
                filterContext.ActionDescriptor.IsDefined(typeof(NonActionAttribute), inherit: true) ||
                filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true))
                return;

            // Authorization must be done at Root action
            if (filterContext.IsChildAction)
                return;

            #endregion Skip Authorization

            AuthorizeResult authorizeResult = AuthorizeUser(filterContext);

            if (authorizeResult == AuthorizeResult.Success)
            {
                // ** IMPORTANT **
                // Since we're performing authorization at the action level, the authorization code runs
                // after the output caching module. In the worst case this could allow an authorized user
                // to cause the page to be cached, then an unauthorized user would later be served the
                // cached page. We work around this by telling proxies not to cache the sensitive page,
                // then we hook our custom authorization code into the caching mechanism so that we have
                // the final say on whether a page should be served from the cache.

                HttpCachePolicyBase cachePolicy = filterContext.HttpContext.Response.Cache;
                cachePolicy.SetProxyMaxAge(new TimeSpan(0));

                // add the authorization handler for cache
                // data is filterContext
                cachePolicy.AddValidationCallback(CacheAuthorizationHandler, filterContext);
            }
            else
            {
                HandleUnauthorizedRequest(filterContext, authorizeResult);

                // Always set the filterContext.Result to stop processing the action
                if (filterContext.Result == null)
                    filterContext.Result = new EmptyResult();
            }
        }

        #endregion OnAuthorization

        #region CacheValidateHandler

        // CacheAuthorizationHandler is only attached to the CachePolicy after first time success authorization

        /// <summary>
        /// When output is cached, the OnAuthorization will be skipped
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="data">The data</param>
        /// <param name="validationStatus">The validation status</param>
        private void CacheAuthorizationHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
            //  when output is cached, the cachePolicy will trigger the CacheValidateHandler to authorize user

            if (data == null)
                throw new ArgumentNullException(nameof(data), "data should be AuthorizationContext");

            AuthorizationContext filterContext = (AuthorizationContext)data;

            if (filterContext == null)
                throw new InvalidCastException("data should be AuthorizationContext");

            AuthorizeResult authorizeResult = AuthorizeUser(filterContext);

            // HttpValidationStatus.Valid => return cache data
            validationStatus = authorizeResult == AuthorizeResult.Success ? HttpValidationStatus.Valid : HttpValidationStatus.IgnoreThisRequest;
        }

        #endregion CacheValidateHandler

        #region HandleUnauthorizedRequest

        /// <summary>
        /// Stop processing the action and return HttpUnauthorizedResult
        /// </summary>
        /// <param name="filterContext">The filter context</param>
        /// <param name="authorizeResult">The authorize result</param>
        protected virtual void HandleUnauthorizedRequest(AuthorizationContext filterContext, AuthorizeResult authorizeResult)
        {
            // let the application handle itself
            //if (authorizeResult == AuthorizeResult.FailedNotLoggedIn)
            //{
            //    filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            //    return;
            //}

            // handle the unauthorized request
            if (ActiveRoleEngine.ProcessUnauthorizedRequest != null)
                ActiveRoleEngine.ProcessUnauthorizedRequest(filterContext, authorizeResult);
            else
                // if the ProcessUnauthorizedRequest is not defined, return 403 forbidden code
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        #endregion HandleUnauthorizedRequest

        #endregion Methods
    }
}
