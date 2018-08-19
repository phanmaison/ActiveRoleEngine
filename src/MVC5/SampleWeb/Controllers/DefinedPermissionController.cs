using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using ActiveRoleEngine;

namespace SampleWeb.Controllers
{
    /// <summary>
    /// Permission can be defined at controller level
    /// </summary>
    /// <seealso cref="SampleWeb.Controllers.ActiveControllerBase" />
    [ActivePermission(Group = "Controller Permission", Description = "All actions in the controller have the same permission")]
    public class ControllerPermissionController : ActiveControllerBase
    {
        // inherit permission from controller
        public ActionResult Index()
        {
            return AuthorizedContent();
        }

        // inherit permission from controller
        public ActionResult Action1()
        {
            return AuthorizedContent();
        }
        // inherit permission from controller
        public ActionResult Action2()
        {
            return AuthorizedContent();
        }

        [AllowAnonymous]
        public ActionResult AllowAnonymous()
        {
            return AuthorizedContent();
        }
    }

    /// <summary>
    /// Permission can be defined at action level
    /// </summary>
    /// <seealso cref="SampleWeb.Controllers.ActiveControllerBase" />
    [ActivePermission(Group = "Action Permission", Description = "Each action has its own permission")]
    public class ActionPermissionController : ActiveControllerBase
    {
        [ActivePermission]
        public ActionResult Index()
        {
            return AuthorizedContent();
        }
        [ActivePermission]
        public ActionResult Action1()
        {
            return AuthorizedContent();
        }
        [ActivePermission]
        public ActionResult Action2()
        {
            return AuthorizedContent();
        }


        [ActivePermission(Permission = "CustomPermissionId", Description = "Action3: PermissionId can be customized")]
        public ActionResult Action3()
        {
            return AuthorizedContent();
        }

        [AllowAnonymous]
        public ActionResult AllowAnonymous()
        {
            return AuthorizedContent();
        }
    }

    [ActivePermission(Group = "Base Permission can be inherited")]
    public abstract class BasePermissionController : ActiveControllerBase
    {
    }

    public class DerivedPermission1Controller : BasePermissionController
    {
        public ActionResult Action1()
        {
            return AuthorizedContent();
        }
        public ActionResult Action2()
        {
            return AuthorizedContent();
        }
        [ActivePermission(Description = "Custom Permission")]
        public ActionResult Action3()
        {
            return AuthorizedContent();
        }
        [ActivePermission(Description = "Custom Permission")]
        public ActionResult Action4()
        {
            return AuthorizedContent();
        }

        [AllowAnonymous]
        public ActionResult AllowAnonymous()
        {
            return AuthorizedContent();
        }
    }


    [ActivePermission(PermissionType = PermissionType.SuperAdmin, Group = "Super Admin", Description = "This controller can be accessed by supper admin only")]
    public class SuperAdminController : ActiveControllerBase
    {
        [ActivePermission(PermissionType = PermissionType.SuperAdmin, Description = "This action can be access by super admin only")]
        public ActionResult Index()
        {
            return AuthorizedContent();
        }

        // inherit permission from controller
        public ActionResult Default()
        {
            return AuthorizedContent();
        }


    }

}
