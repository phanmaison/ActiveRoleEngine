using System;
using System.Reflection;
using System.Web.Mvc;

namespace ActiveRoleEngine
{
    /// <inheritdoc />
    /// <summary>
    /// Permission information
    /// </summary>
    public class PermissionModel : IPermissionAttribute
    {
        // sync the IPermissionInfo from ActivePermissionAttribute

        #region IPermissionInfo

        #region Permission

        /// <inheritdoc />
        /// <summary>
        /// Custom permission id
        /// <para>If provide, it will override the area/controller/action</para>
        /// </summary>
        /// <value>
        /// The permission identifier.
        /// </value>
        public string Permission { get; }

        #endregion Permission

        #region Area

        /// <inheritdoc />
        /// <summary>
        /// Area Name if any (default from controller)
        /// </summary>
        /// <value>
        /// The area.
        /// </value>
        public string Area { get; }

        #endregion Area

        #region Controller

        /// <inheritdoc />
        /// <summary>
        /// Name of Controller if any (default from Controller)
        /// </summary>
        /// <value>
        /// The controller.
        /// </value>
        public string Controller { get; }

        #endregion Controller

        #region Action

        /// <inheritdoc />
        /// <summary>
        /// Name of Action if any (default from Action Name)
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public string Action { get; }

        #endregion Action

        #region Group

        /// <inheritdoc />
        /// <summary>
        /// Group of permission for grouping on UI (Action will inherit from Controller if not defined)
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public string Group { get; set; }

        #endregion Group

        #region Description

        /// <inheritdoc />
        /// <summary>
        /// Description of the permission
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        #endregion Description

        #region PermissionType

        /// <inheritdoc />
        /// <summary>
        /// Type of permission
        /// </summary>
        public PermissionType PermissionType { get; } = PermissionType.Permisson;

        #endregion PermissionType

        #region PermissionId

        /// <inheritdoc />
        /// <summary>
        /// Unique key of the permission
        /// </summary>
        /// <value>
        /// The unique key identifier.
        /// </value>
        public string PermissionId => ActiveRoleEngineHelper.GetPermissionId(this.Permission, this.Area, this.Controller, this.Action);

        #endregion PermissionId

        #endregion IPermissionInfo

        #region PermissionModel

        ///// <summary>
        ///// If this permission model is for superadmin only
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if this instance is system admin; otherwise, <c>false</c>
        ///// </value>
        //public bool IsSuperAdmin { get; set; }


        #endregion PermissionModel

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionModel"/> class.
        /// </summary>
        public PermissionModel()
        {

        }

        /// <summary>
        /// Initializes a new instance of <see cref="PermissionModel"/> for Controller
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="controllerType">Type of the controller.</param>
        public PermissionModel(IPermissionAttribute attribute, Type controllerType)
        {
            this.Permission = attribute.Permission;
            this.Area = attribute.Area.IsNullOrWhiteSpace() ?
                ActiveRoleEngineHelper.GetControllerArea(controllerType) :
                attribute.Area;
            this.Controller = attribute.Controller.IsNullOrWhiteSpace() ?
                ActiveRoleEngineHelper.GetControllerName(controllerType) :
                attribute.Controller;
            this.Action = attribute.Action;

            this.Group = attribute.Group;
            this.Description = attribute.Description;
            this.PermissionType = attribute.PermissionType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionModel" /> class for Action
        /// </summary>
        /// <param name="attribute">The attribute</param>
        /// <param name="controllerAttribute">The controller attribute if any</param>
        /// <param name="controllerType">Type of the controller</param>
        /// <param name="methodInfo">The method information</param>
        public PermissionModel(IPermissionAttribute attribute,
            IPermissionAttribute controllerAttribute,
            Type controllerType,
            MemberInfo methodInfo)
        {
            // if the Action is decorated by ActivePermissionAttribute
            // it will be considered a new permission (unless it map to an existing permission)


            // get permission decoration from ActivePermissionAttribute
            // if ActivePermissionAttribute does not provide the information
            // then get from MethodInfo or controllerAttribute


            // not inherit from controller, the action must define its own
            this.Permission = attribute.Permission;

            // inherit from controller if not defined
            // Area can be "" (root area) => here we check null only
            this.Area = attribute.Area ?? ActiveRoleEngineHelper.GetControllerArea(controllerType);

            // inherit from controller if not defined
            // Controller must be specified

            if (attribute.Controller.IsNotNullOrEmpty())
            {
                this.Controller = attribute.Controller;
            }
            else
            {
                this.Controller = controllerAttribute == null || controllerAttribute.Controller.IsNullOrWhiteSpace() ?
                    ActiveRoleEngineHelper.GetControllerName(controllerType) :
                    controllerAttribute.Controller;
            }

            if (attribute.Action.IsNotNullOrEmpty())
            {
                this.Action = attribute.Action;
            }
            else
            {
                string actionName = null;

                if (methodInfo.IsDefined(typeof(ActionNameAttribute)))
                    actionName = methodInfo.GetCustomAttribute<ActionNameAttribute>()?.Name;

                if (actionName.IsNullOrEmpty())
                    actionName = methodInfo.Name;

                this.Action = actionName;
            }

            // the Action does not define the group => inherit from controller
            this.Group = attribute.Group.IsNullOrWhiteSpace() ? controllerAttribute?.Group : attribute.Group;

            this.Description = attribute.Description;

            this.PermissionType = attribute.PermissionType;

            //// if AllowSuperAdminOnlyEnum.Inherit then get from controller
            //this.PermissionType = attribute.PermissionType == PermissionType.Inherit ?
            //    (controllerAttribute?.PermissionType ?? PermissionType.Inherit) :
            //    attribute.PermissionType;
        }

        #endregion Constructor
    }
}
