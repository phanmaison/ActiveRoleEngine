using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SampleWeb.Controllers;

namespace SampleWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AuthConfig.ConfigRoleEngine();


            // For testing purpose: always import permission when start application
            RoleEngineController.ImportPermission();

        }

        #region Application_Error

        /// <summary>
        /// Global handler for all exceptions
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Application_Error(object sender, EventArgs e)
        {
        }

        #endregion Application_Error

        private bool IsAjaxRequest()
        {
            //The easy way
            var isAjaxRequest = (Request["X-Requested-With"] == "XMLHttpRequest")
                                || (Request.Headers["X-Requested-With"] == "XMLHttpRequest");

            //If we are not sure that we have an AJAX request or that we have to return JSON
            //we fall back to Reflection
            if (!isAjaxRequest)
            {
                try
                {
                    //The controller and action
                    var controllerName = Request.RequestContext.
                        RouteData.Values["controller"].ToString();
                    var actionName = Request.RequestContext.
                        RouteData.Values["action"].ToString();

                    //We create a controller instance
                    var controllerFactory = new DefaultControllerFactory();
                    var controller = controllerFactory.CreateController(
                        Request.RequestContext, controllerName) as Controller;

                    //We get the controller actions
                    var controllerDescriptor =
                        new ReflectedControllerDescriptor(controller.GetType());
                    var controllerActions =
                        controllerDescriptor.GetCanonicalActions();

                    //We search for our action
                    foreach (var actionDescriptor1 in controllerActions)
                    {
                        var actionDescriptor = (ReflectedActionDescriptor)actionDescriptor1;
                        if (actionDescriptor.ActionName.ToUpper().Equals(actionName.ToUpper()))
                        {
                            //If the action returns JsonResult then we have an AJAX request
                            if (actionDescriptor.MethodInfo.ReturnType == typeof(JsonResult))
                                return true;
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }

            return isAjaxRequest;
        }

        #region Session_Start

        // https://stackoverflow.com/questions/32225942/sessionid-changing-at-every-page-call
        // init the session
        protected void Session_Start(Object sender, EventArgs e)
        {
            /*
             * When using cookie - based session state, ASP.NET does not allocate 
             * storage for session data until the Session object is used.
             * As a result, a new session ID is generated for each page request 
             * until the session object is accessed. If your application requires 
             * a static session ID for the entire session, you can either 
             * implement the Session_Start method in the application's Global.asax 
             * file and store data in the Session object to fix the session ID, 
             * or you can use code in another part of your application to 
             * explicitly store data in the Session object. */

            Session["init"] = 0;
        }

        #endregion Session_Start
    }
}
