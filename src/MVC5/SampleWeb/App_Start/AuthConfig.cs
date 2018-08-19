using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using SampleWeb.Controllers;
using System.Collections.Generic;
using System.Net;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using ActiveRoleEngine;

[assembly: OwinStartup(typeof(SampleWeb.AuthConfig))]
namespace SampleWeb
{
    public class AuthConfig
    {
        #region Owin

        /// <summary>
        /// Name of cookie that stores the user information
        /// </summary>
        public const string AUTHEN_COOKIE = "SampleAuth";

        /// <summary>
        /// Configure Owin
        /// </summary>
        /// <param name="app">The application.</param>
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/account/login"),
                CookieName = AUTHEN_COOKIE,
                SlidingExpiration = true,
                ReturnUrlParameter = "r",
            });

            // must set the UniqueClaimTypeIdentifier if the ClaimsIdentity use different token name
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ActiveUserEngine.CLAIM_USERNAME;
        }

        #endregion Owin

        #region RoleEngine

        /// <summary>
        /// Configure the role engine
        /// </summary>
        public static void ConfigRoleEngine()
        {
            ActiveRoleEngine.ActiveRoleEngine.ConfigRoleEngine(
                CurrentUser.RestoreUserSession,
                AuthConfig.HandleUnauthorizedRequest);
        }


        #endregion RoleEngine

        #region HandleUnauthorizedRequest

        /// <summary>
        /// Handles the unauthorized request.
        /// </summary>
        /// <param name="filterContext">The filter context</param>
        /// <param name="authorizeResult">The authorize result</param>
        private static void HandleUnauthorizedRequest(AuthorizationContext filterContext, AuthorizeResult authorizeResult)
        {
            switch (authorizeResult)
            {
                case AuthorizeResult.FailedNotLoggedIn:
                    // User not logged in => redirect to login page by setting the error 401
                    // Owin will automatically redirect all 401 errors to login page
                    filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                    break;
                case AuthorizeResult.PermissionNotDefined:
                    ProcessUnauthorizedError(filterContext, $"Permission is not defined for {filterContext.Controller.GetType().Name}/{filterContext.ActionDescriptor.ActionName}");
                    break;
                case AuthorizeResult.FailedSuperAdminOnly:
                    ProcessUnauthorizedError(filterContext, "This feature is available for system admin only");
                    break;
                // AuthorizeResult.NotAuthorized
                default:
                    ProcessUnauthorizedError(filterContext, $"You are unauthorized to access to this feature");
                    break;
            }
        }

        /// <summary>
        /// Processes the unauthorized error.
        /// </summary>
        /// <param name="filterContext">The filter context</param>
        /// <param name="message">The message to be displayed on screen</param>
        private static void ProcessUnauthorizedError(AuthorizationContext filterContext, string message)
        {
            // execute the specified controller so that the Url is not redirected
            IController controller = new ErrorController();

            RouteData routeData = new RouteData();
            routeData.DataTokens["area"] = "";
            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = "Unauthorized";
            routeData.Values["message"] = message;

            RequestContext rc = new RequestContext(filterContext.HttpContext, routeData);

            // execute the error controller and add the content to response stream
            controller.Execute(rc);
        }

        #endregion HandleUnauthorizedRequest
    }
}