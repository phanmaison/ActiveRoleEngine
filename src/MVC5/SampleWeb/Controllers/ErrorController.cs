using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SampleWeb.Controllers
{
    /// <summary>
    /// Error handling controller
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    [AllowAnonymous]
    public class ErrorController : ActiveControllerBase
    {
        /// <summary>
        /// IMPORTANT this action is used by Role Engine to handle unauthorized access
        /// <para></para>
        /// </summary>
        public ActionResult Unauthorized()
        {
            // Refer to AuthConfig for setup of RoleEngine

            ViewBag.Title = "Unauthorized";
            ViewBag.ErrorCode = 403;

            this.RouteData.Values.TryGetValue("message", out object message);

            ViewBag.Message = message;

            return ActiveView();
        }
    }
}