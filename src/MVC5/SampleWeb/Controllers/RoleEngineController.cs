using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using ActiveRoleEngine;
using SampleWeb.DbContext;
using SampleWeb.Domain;
using SampleWeb.Models;

namespace SampleWeb.Controllers
{
    [ActivePermission(Group = "Role Engine", Permission = "Admin", Description = "Manage role engine")]
    [Authorize(Users = "Admin, Manager", Roles = "Manager")]
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

        #region Permission


        /// <summary>
        /// List the database permission
        /// </summary>
        /// <returns></returns>
        public ActionResult Permission()
        {
            return ActiveView();
        }

        /// <summary>
        /// Import permissions from defined permissions
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PermissionImport()
        {
            // copy permissions from ActiveRoleEngine.NonSuperAdminPermissions (source code defined permissions)
            SampleDbContext.Current.Permissions.Clear();

            SampleDbContext.Current.Permissions.AddRange(ActiveRoleEngine.ActiveRoleEngine.NonSuperAdminPermissions
                    .Select(p => new SysPermission
                    {
                        PermissionId = p.PermissionId,
                        PermissionName = p.PermissionId,
                        PermissionGroup = p.Group,
                        Description = p.Description
                    }));

            return ActiveView("~/Views/RoleEngine/Permission.cshtml");
        }

        #endregion Permission

        #region Role

        /// <summary>
        /// List of role
        /// </summary>
        /// <returns></returns>
        public ActionResult Role()
        {
            return ActiveView();
        }

        /// <summary>
        /// Add screen
        /// </summary>
        /// <returns></returns>
        public ActionResult RoleAdd()
        {
            ViewBag.Title = "Role Add";

            return ActiveView("~/Views/RoleEngine/RoleAddEdit.cshtml");
        }

        /// <summary>
        /// Edit screen
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <returns></returns>
        public ActionResult RoleEdit(string id)
        {
            SysRole role = SampleDbContext.Current.Roles.FirstOrDefault(r => r.RoleId.ToString() == id);

            // role not found => return to list
            if (role == null)
                return RedirectToAction("Role");

            ViewBag.Title = "Role Edit - " + role.RoleName;

            RoleAddEditModel model = new RoleAddEditModel
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                Permissions = SampleDbContext.Current.RolePermissions
                    .Where(rp => rp.RoleId == role.RoleId)
                    .Select(rp => rp.PermissionId).ToList()
            };

            return ActiveView("~/Views/RoleEngine/RoleAddEdit.cshtml", model);
        }

        /// <summary>
        /// Save add/edit role
        /// </summary>
        /// <param name="model">The model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleSave(RoleAddEditModel model)
        {
            if (!ModelState.IsValid)
            {
                return ActiveView("~/Views/RoleEngine/RoleAddEdit.cshtml");
            }

            if (model.RoleId != null)
            {
                // Edit save

                SysRole role = SampleDbContext.Current.Roles.FirstOrDefault(r => r.RoleId == model.RoleId.Value);

                if (role == null)
                {
                    ModelState.AddModelError("", "RoleId not found");
                    return ActiveView("~/Views/RoleEngine/RoleAddEdit.cshtml", model);
                }

                // update role
                role.RoleName = model.RoleName;
                role.Description = model.Description;

                // update role_permission
                SampleDbContext.Current.RolePermissions.RemoveAll(rp => rp.RoleId == role.RoleId);
                if (model.Permissions != null && model.Permissions.Count > 0)
                {
                    SampleDbContext.Current.RolePermissions.AddRange(model.Permissions.Select(p => new SysRolePermission
                    {
                        RoleId = role.RoleId,
                        PermissionId = p
                    }));
                }

                return RedirectToAction("Role");
            }

            // Add save

            SysRole newRole = new SysRole
            {
                RoleId = Guid.NewGuid(),
                RoleName = model.RoleName,
                Description = model.Description
            };

            SampleDbContext.Current.Roles.Add(newRole);
            if (model.Permissions != null && model.Permissions.Count > 0)
            {
                SampleDbContext.Current.RolePermissions.AddRange(model.Permissions.Select(p => new SysRolePermission
                {
                    RoleId = newRole.RoleId,
                    PermissionId = p
                }));
            }

            return RedirectToAction("Role");
        }

        #endregion Role

        #region User

        public new ActionResult User()
        {
            return ActiveView();
        }

        public ActionResult UserAdd()
        {
            ViewBag.Title = "User Add";

            UserAddEditModel model = new UserAddEditModel();

            return ActiveView("~/Views/RoleEngine/UserAddEdit.cshtml", model);
        }
        public ActionResult UserEdit(string id)
        {
            SysUser user = SampleDbContext.Current.Users.FirstOrDefault(u => u.UserId.ToString() == id);

            // user not found => return to list
            if (user == null)
                return RedirectToAction("User");

            ViewBag.Title = "User Edit - " + user.Username;

            UserAddEditModel model = new UserAddEditModel
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                IsSuperAdmin = user.IsSuperAdmin,
                Roles = SampleDbContext.Current.UserRoles
                    .Where(ur => ur.UserId == user.UserId)
                    .Select(ur => ur.RoleId).ToList()
            };

            return ActiveView("~/Views/RoleEngine/UserAddEdit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserSave(UserAddEditModel model)
        {
            if (!ModelState.IsValid)
            {
                return ActiveView("~/Views/RoleEngine/UserAddEdit.cshtml");
            }

            if (model.UserId != null)
            {
                // edit save

                SysUser user = SampleDbContext.Current.Users.FirstOrDefault(u => u.UserId == model.UserId.Value);

                if (user == null)
                {
                    ModelState.AddModelError("", "UserId not found");
                    return ActiveView("~/Views/RoleEngine/UserAddEdit.cshtml", model);
                }

                // Update User
                user.Username = model.Username;
                user.FullName = model.FullName;
                //user.IsSuperAdmin = model.IsSuperAdmin;

                // Update User Role
                SampleDbContext.Current.UserRoles.RemoveAll(ur => ur.UserId == user.UserId);

                if (model.Roles != null && model.Roles.Count > 0)
                {
                    SampleDbContext.Current.UserRoles.AddRange(model.Roles.Select(r => new SysUserRole()
                    {
                        UserId = user.UserId,
                        RoleId = r
                    }));
                }

                return RedirectToAction("User");
            }

            // Add save

            SysUser newUser = new SysUser
            {
                UserId = Guid.NewGuid(),
                Username = model.Username,
                FullName = model.FullName,
                IsSuperAdmin = model.IsSuperAdmin
            };

            SampleDbContext.Current.Users.Add(newUser);

            if (model.Roles != null && model.Roles.Count > 0)
            {
                SampleDbContext.Current.UserRoles.AddRange(model.Roles.Select(r => new SysUserRole()
                {
                    UserId = newUser.UserId,
                    RoleId = r
                }));
            }

            return RedirectToAction("User");
        }

        public ActionResult UserView(string id)
        {
            SysUser user = SampleDbContext.Current.Users.FirstOrDefault(u => u.UserId.ToString() == id);

            // user not found => return to list
            if (user == null)
                return RedirectToAction("User");

            ViewBag.Title = "User View - " + user.Username;

            UserAddEditModel model = new UserAddEditModel
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                IsSuperAdmin = user.IsSuperAdmin,
                Roles = SampleDbContext.Current.UserRoles
                    .Where(ur => ur.UserId == user.UserId)
                    .Select(ur => ur.RoleId).ToList()
            };

            return ActiveView("~/Views/RoleEngine/UserView.cshtml", model);
        }

        public ActionResult UserPermission(string id)
        {
            SysUser user = SampleDbContext.Current.Users.FirstOrDefault(u => u.UserId.ToString() == id);

            // user not found => return to list
            if (user == null)
                return RedirectToAction("User");

            ViewBag.Title = "User Permission - " + user.Username;

            UserAddEditModel model = new UserAddEditModel
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
            };

            return ActiveView("~/Views/RoleEngine/UserPermission.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserPermission(UserPermissionSaveModel model)
        {
            SysUser user = SampleDbContext.Current.Users.FirstOrDefault(u => u.UserId == model.UserId);

            // user not found => return to list
            if (user == null)
                return RedirectToAction("User");

            // save the User Permission
            SampleDbContext.Current.UserPermissions.RemoveAll(up => up.UserId == user.UserId);

            if (model.UserPermissions != null && model.UserPermissions.Count > 0)
            {
                foreach (var userPermission in model.UserPermissions)
                {
                    if (userPermission.Value == "Add")
                    {
                        SampleDbContext.Current.UserPermissions.Add(new SysUserPermission
                        {
                            UserId = user.UserId,
                            PermissionId = userPermission.PermissionId,
                            IsAdd = true
                        });
                    }
                    else if (userPermission.Value == "Remove")
                    {
                        SampleDbContext.Current.UserPermissions.Add(new SysUserPermission
                        {
                            UserId = user.UserId,
                            PermissionId = userPermission.PermissionId,
                            IsAdd = false
                        });
                    }
                    // inherit => ignore
                }
            }

            return RedirectToAction("UserView", "RoleEngine", new RouteValueDictionary { { "Id", model.UserId.ToString() } });
        }

        #endregion User

    }
}
