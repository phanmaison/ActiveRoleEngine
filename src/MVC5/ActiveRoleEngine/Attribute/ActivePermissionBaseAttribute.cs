using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Web.Mvc;
using ActiveFramework.RoleEngine;
using System.Web;
using System.Security.Principal;
using System.Net;
using System.Web.Routing;
using System.Web.Mvc.Async;

namespace ActiveFramework
{
    /// <summary>
    /// Abstract class for permission
    /// </summary>
    /// <seealso cref="System.Web.Mvc.FilterAttribute" />
    /// <seealso cref="System.Web.Mvc.IAuthorizationFilter" />
    /// <seealso cref="ActiveFramework.RoleEngine.IDBPermission" />
    public abstract class ActivePermissionBaseAttribute : FilterAttribute, IAuthorizationFilter, IDBPermission
    {
        /// <summary>
        /// Type of permission
        /// </summary>
        protected PermissionTypeEnum PermissionType { get; set; } = PermissionTypeEnum.Permisson;

        /// <summary>
        /// UniqueId of the permission
        /// </summary>
        /// <value>
        /// The permission identifier
        /// </value>
        public abstract string PermissionId { get; }

        /// <summary>
        /// Group name of permission (for grouping on UI)
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public string Group { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

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
        protected virtual AuthorizeResultEnum AuthorizeUser(AuthorizationContext filterContext)
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
                return AuthorizeResultEnum.Failed_NotLoggedIn;

            // TODO: temp disable
            //// always authorize system admin
            //if (user.IsSystemAdmin)
            //    return AuthorizeResultEnum.Success;

            // FROM NOW, USER IS NOT SUPER ADMIN


            // TODO:
            // https://www.ryadel.com/en/asp-net-mvc-fix-ambiguous-action-methods-errors-multiple-action-methods-action-name-c-sharp-core/
            // Get the MethodInfo of the action
            // https://stackoverflow.com/questions/1972234/how-do-i-get-a-methodinfo-from-an-actionexecutingcontext

            MethodInfo methodInfo = filterContext.ActionDescriptor.GetMethodInfo();
            Type controllerType = filterContext.ActionDescriptor.ControllerDescriptor.ControllerType;

            PermissionModel permission = null;

            //
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

                string actionNamespace = RoleEngineHelper.GetDebugNamespace(methodInfo);

                throw new SystemException($"FATAL ERROR: cannot found the defined permission for '{actionNamespace}'");
            }

            // to display the permission on error page
            this.AuthorizingPermissionId = permission.PermissionId;

            switch (permission.PermissionType)
            {
                case PermissionTypeEnum.AuthorizedOnly:
                    // ActiveUserEngine.CurrentUser is not null => Authenticated
                    return AuthorizeResultEnum.Success;
                case PermissionTypeEnum.SuperAdmin:
                    return user.IsSystemAdmin ? AuthorizeResultEnum.Success : AuthorizeResultEnum.Failed_SuperAdminOnly;
                case PermissionTypeEnum.Permisson:
                default:
                    if (user.Permissions == null || !user.Permissions.Contains(permission.PermissionId))
                        return AuthorizeResultEnum.Failed_NotAuthorized;

                    return AuthorizeResultEnum.Success;
            }

        }

        #endregion AuthorizeUser

        #region OnAuthorization

        /// <summary>
        /// Called when authorization is required.
        /// </summary>
        /// <param name="filterContext">The filter context</param>
        /// <exception cref="System.ArgumentNullException">filterContext</exception>
        /// <exception cref="System.InvalidOperationException">ActivePermissionAttribute: Cannot Use Within Child Action Cache</exception>
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

            AuthorizeResultEnum authorizeResult = AuthorizeUser(filterContext);

            if (authorizeResult == AuthorizeResultEnum.Success)
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

            AuthorizeResultEnum authorizeResult = AuthorizeUser(filterContext);

            // HttpValidationStatus.Valid => return cache data
            validationStatus = authorizeResult == AuthorizeResultEnum.Success ? HttpValidationStatus.Valid : HttpValidationStatus.IgnoreThisRequest;
        }

        #endregion CacheValidateHandler

        #region HandleUnauthorizedRequest

        /// <summary>
        /// Stop processing the action and return HttpUnauthorizedResult
        /// </summary>
        /// <param name="filterContext">The filter context</param>
        /// <param name="authorizeResult">The authorize result</param>
        protected virtual void HandleUnauthorizedRequest(AuthorizationContext filterContext, AuthorizeResultEnum authorizeResult)
        {
            switch (authorizeResult)
            {
                case AuthorizeResultEnum.Failed_NotLoggedIn:
                    // User not logged in => redirect to login page by setting the error 401
                    // Owin will automatically redirect all 401 errors to login page
                    filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                    break;
                case AuthorizeResultEnum.PermissionNotDefined:
                    ProcessUnauthorizedError(filterContext, "Permission is not defined");
                    break;
                case AuthorizeResultEnum.Failed_SuperAdminOnly:
                    ProcessUnauthorizedError(filterContext, "This functionality is available for system admin only");
                    break;
                // AuthorizeResultEnum.NotAuthorized
                default:
                    ProcessUnauthorizedError(filterContext, $"You need to be granted permission '{this.AuthorizingPermissionId}' to access this feature. Please contact administrator for support");
                    break;
            }

        }

        #endregion HandleUnauthorizedRequest

        #region ProcessUnauthorizedError

        /// <summary>
        /// Processes the unauthorized error.
        /// </summary>
        /// <param name="filterContext">The filter context</param>
        /// <param name="message">The message</param>
        private void ProcessUnauthorizedError(AuthorizationContext filterContext, string message)
        {
            // If the handler is defined => execute it
            if (ActiveRoleEngine.UnauthorizedHandlerModel != null)
            {
                // execute the specified controller (provided by the web application)
                // so that the Url is not redirected

                IController controller = (IController)Activator.CreateInstance(ActiveRoleEngine.UnauthorizedHandlerModel.ControllerType);

                RouteData routeData = new RouteData();
                routeData.DataTokens["area"] = ActiveRoleEngine.UnauthorizedHandlerModel.Area;
                routeData.Values["controller"] = ActiveRoleEngine.UnauthorizedHandlerModel.Controller;
                routeData.Values["action"] = ActiveRoleEngine.UnauthorizedHandlerModel.Action;
                routeData.Values["permissionId"] = this.AuthorizingPermissionId;
                routeData.Values["message"] = message;

                RequestContext rc = new RequestContext(filterContext.HttpContext, routeData);

                /* This will run specific action without redirecting */
                // execute the error controller and add the content to response stream
                controller.Execute(rc);

                // then set a not-null value here to stop the processing
                filterContext.Result = new EmptyResult();

                //filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden, message);
            }
            // Else, fallback to 403 Forbidden error
            else
            {
                // IMPORTANT: 401 code is used Owin, whenever Owin detect a 401 code, it will redirect the user to login page although user has logged in
                // Hence, we return 403 code instead

                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden, message);
            }
        }

        #endregion ProcessUnauthorizedError

        #endregion Methods


    }
}
