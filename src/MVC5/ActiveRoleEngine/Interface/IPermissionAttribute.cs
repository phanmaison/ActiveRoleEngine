namespace ActiveRoleEngine
{
    /// <inheritdoc />
    /// <summary>
    /// Permission attribute model
    /// </summary>
    public interface IPermissionAttribute : IDbPermission
    {
        /// <summary>
        /// Gets or sets the area.
        /// </summary>
        /// <value>
        /// The area.
        /// </value>
        string Area { get; }

        /// <summary>
        /// Gets or sets the controller.
        /// </summary>
        /// <value>
        /// The controller.
        /// </value>
        string Controller { get; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        string Action { get; }

        /// <summary>
        /// Once defined, the permission will override the area/controller/action
        /// </summary>
        /// <value>
        /// The permission
        /// </value>
        string Permission { get; }

        /// <summary>
        /// Type of permission
        /// </summary>
        /// <value>
        /// The type of the permission.
        /// </value>
        PermissionType PermissionType { get; }
    }
}
