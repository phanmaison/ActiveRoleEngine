using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ActiveRoleEngine;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SampleWeb.DbContext;
using SampleWeb.Models;

namespace SampleWeb.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public AccountController()
        {
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string r)
        {
            ViewBag.ReturnUrl = r;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // validate user login
            var sysUser = SampleDbContext.Current.Users.FirstOrDefault(x => x.Username == model.Username);

            if (sysUser == null)
            {
                ModelState.AddModelError("", "Invalid username");
                return View(model);
            }

            ActiveUserEngine.LoginUser(Request, sysUser, model.RememberMe);

            return RedirectToLocal(returnUrl);
        }

        public ActionResult LogOff()
        {
            ActiveUserEngine.LogoutUser(Request);

            return RedirectToAction("Index", "Home");
        }

        #region Helpers

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}