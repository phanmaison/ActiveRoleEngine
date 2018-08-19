using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SampleWeb.Domain;

namespace SampleWeb.DbContext
{
    public class SampleDbContext
    {
        public List<SysUser> Users { get; } = new List<SysUser>();
        public List<SysRole> Roles { get; } = new List<SysRole>();
        public List<SysPermission> Permissions { get; } = new List<SysPermission>();
        public List<SysUserRole> UserRoles { get; } = new List<SysUserRole>();
        public List<SysRolePermission> RolePermissions { get; } = new List<SysRolePermission>();
        public List<SysUserPermission> UserPermissions { get; } = new List<SysUserPermission>();

        private SampleDbContext()
        {
            // Initialize the data

            Guid saGuid = Guid.NewGuid();

            Users.Add(new SysUser
            {
                Username = "sa",
                UserId = saGuid,
                Password = "",
                FullName = "Super Admin",
                IsSuperAdmin = true
            });

            Users.Add(new SysUser
            {
                Username = "user1",
                UserId = Guid.NewGuid(),
                Password = "",
                FullName = "user1"
            });

            Users.Add(new SysUser
            {
                Username = "user2",
                UserId = Guid.NewGuid(),
                Password = "",
                FullName = "user2"
            });

            Users.Add(new SysUser
            {
                Username = "user3",
                UserId = Guid.NewGuid(),
                Password = "",
                FullName = "user3"
            });
        }

        public static SampleDbContext Current { get; } = new SampleDbContext();
    }
}
