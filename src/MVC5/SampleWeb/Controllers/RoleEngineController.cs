using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using ActiveRoleEngine;

namespace SampleWeb.Controllers
{
    [ActivePermission(Group = "Role Engine", Permission = "Admin", Description = "Manage role engine")]
    public class RoleEngineController : ActiveControllerBase
    {

        // GET: RoleBase
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Defined Permission";

            return ActiveView();
        }

        [AllowAnonymous]
        public ActionResult AllAction()
        {
            ViewBag.Title = "All Actions";

            return ActiveView();
        }


        public ActionResult Permission()
        {

            return ActiveView();
        }


    }
}
