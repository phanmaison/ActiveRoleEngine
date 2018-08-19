namespace ActiveRoleEngine
{
    /// <summary>
    /// Database permission model
    /// </summary>
    public interface IDbPermission
    {
        /// <summary>
        /// UniqueId of the permission
        /// </summary>
        /// <value>
        /// The permission identifier
        /// </value>
        string PermissionId { get; }

        /// <summary>
        /// Group name of permission (for grouping on UI)
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        string Group { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        string Description { get; set; }
    }
}
