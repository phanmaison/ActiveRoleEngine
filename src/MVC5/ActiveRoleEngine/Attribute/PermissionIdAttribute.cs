using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveFramework
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ActiveFramework.ActivePermissionBaseAttribute" />
    public class PermissionIdAttribute : ActivePermissionBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionIdAttribute"/> class
        /// </summary>
        public PermissionIdAttribute()
        {
            this.PermissionType = PermissionTypeEnum.Permisson;
        }

        /// <summary>
        /// Gets or sets the permission
        /// </summary>
        /// <value>
        /// The permission
        /// </value>
        public string Permission { get; set; }

        /// <summary>
        /// UniqueId of the permission
        /// </summary>
        /// <value>
        /// The permission identifier
        /// </value>
        public override string PermissionId => Permission;
    }
}
