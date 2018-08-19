using ActiveFramework.RoleEngine;
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
    public class PermissionActionAttribute : ActivePermissionBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionActionAttribute" /> class
        /// </summary>
        public PermissionActionAttribute()
        {
            this.PermissionType = PermissionTypeEnum.Action;
        }

        #region Area

        /// <summary>
        /// Area Name if any (default from controller)
        /// </summary>
        /// <value>
        /// The area.
        /// </value>
        public string Area { get; set; }

        #endregion Area

        #region Controller

        /// <summary>
        /// Name of Controller if any (default from Controller)
        /// </summary>
        /// <value>
        /// The controller.
        /// </value>
        public string Controller { get; set; }

        #endregion Controller

        #region Action

        /// <summary>
        /// Name of Action if any (default from Action Name)
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public string Action { get; set; }

        #endregion Action

        /// <summary>
        /// UniqueId of the permission
        /// </summary>
        /// <value>
        /// The permission identifier
        /// </value>
        public override string PermissionId => RoleEngineHelper.GetPermissionId(string.Empty, this.Area, this.Controller, this.Action);
    }
}
