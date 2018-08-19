namespace ActiveRoleEngine
{
    /// <inheritdoc />
    /// <summary>
    /// User account model
    /// </summary>
    public interface IUserAccount : IUserPermission
    {
        /// <summary>
        /// Gets the user id
        /// </summary>
        object UserId { get; }

        /// <summary>
        /// Gets the username
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Gets the full name of the user.
        /// </summary>
        string FullName { get; }
    }
}
