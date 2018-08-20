using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleWeb.Models
{
    public class UserPermissionModel
    {
        public string PermissionId { get; set; }
        public string Value { get; set; }

    }


    public class UserPermissionSaveModel
    {
        public Guid UserId { get; set; }

        public List<UserPermissionModel> UserPermissions { get; set; }

    }
}
