namespace ActiveRoleEngine
{
    /// <summary>
    /// Result of Authorization
    /// </summary>
    public enum AuthorizeResult
    {
        /// <summary>
        /// Success authorization
        /// </summary>
        Success,
        /// <summary>
        /// Failed: User not logged in
        /// </summary>
        FailedNotLoggedIn,
        /// <summary>
        /// Failed: Only Super Admin is authorized
        /// </summary>
        FailedSuperAdminOnly,
        /// <summary>
        /// Failed: User does not have permission to access
        /// </summary>
        FailedNotAuthorized,
        /// <summary>
        /// Permission is not defined (should be exception)
        /// </summary>
        PermissionNotDefined
    }
}
