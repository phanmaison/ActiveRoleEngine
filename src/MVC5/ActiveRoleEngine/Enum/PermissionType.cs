namespace ActiveRoleEngine
{
    /// <summary>
    /// Type of permission
    /// </summary>
    public enum PermissionType
    {
        /// <summary>
        /// Default type, user need to be granted permission in order to access
        /// </summary>
        Permisson,
        /// <summary>
        /// Defined based on the area, controller and action
        /// </summary>
        Action,
        /// <summary>
        /// Only available for System Admin
        /// </summary>
        SuperAdmin,
        /// <summary>
        /// No permission need to be granted, all authenticated users can access
        /// </summary>
        Authorized,
        /// <summary>
        /// Only internal users can access
        /// </summary>
        AuthorizedInternal,
        /// <summary>
        /// Only external user can access
        /// </summary>
        AuthorizedExternal
    }
}
