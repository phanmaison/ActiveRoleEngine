using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SampleWeb.Controllers
{
    public abstract class ActiveControllerBase : Controller
    {
        #region System.Web

        #region CurrentHttpContext

        /// <summary>
        /// Gets the current HTTP context.
        /// </summary>
        /// <value>
        /// The current HTTP context.
        /// </value>
        protected HttpContext CurrentHttpContext => System.Web.HttpContext.Current;

        #endregion CurrentHttpContext

        #endregion System.Web

        #region ActiveView

        /// <summary>
        /// Return the view depend of request type (Ajax or normal request)
        /// </summary>
        /// <param name="viewPath">The view path.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        protected ActionResult ActiveView(string viewPath = "", object model = null)
        {
            if (Request.IsAjaxRequest())
                return PartialView(viewPath, model);
            else
                return View(viewPath, model);
        }

        #endregion ActiveView

        protected ActionResult AuthorizedContent()
        {
            ViewBag.Title = Request.RawUrl;

            string content = $"You are authorized to access {Request.RawUrl}";

            return Content(content);
        }

    }
}
