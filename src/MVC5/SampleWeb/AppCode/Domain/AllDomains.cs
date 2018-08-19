using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActiveRoleEngine;

namespace SampleWeb.Domain
{
    public class SysUser : IUserAccount
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }

        public string Password { get; set; }
        public string FullName { get; set; }
        public bool IsSuperAdmin { get; set; }

        #region IUserAccount

        string IUserAccount.UserName => Username;

        public bool IsInternalUser { get; } = false;

        public bool IsExternalUser { get; } = false;

        public List<string> Permissions { get; set; }

        object IUserAccount.UserId => this.UserId;

        #endregion IUserAccount
    }

    public class SysRole
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
    }

    public class SysPermission
    {
        public string PermissionId { get; set; }

        public string PermissionName { get; set; }
        public string PermissionGroup { get; set; }
        public string Description { get; set; }
    }

    public class SysUserPermission
    {
        public Guid UserId { get; set; }
        public string PermissionId { get; set; }
        public bool IsAdd { get; set; }
    }

    public class SysUserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }

    public class SysRolePermission
    {
        public Guid RoleId { get; set; }
        public string PermissionId { get; set; }
        
    }
}
