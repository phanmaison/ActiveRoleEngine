using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace ActiveRoleEngine
{
    /// <summary>
    /// Engine for working with Role and Permission
    /// <para>Permissions are defined in controller / action with ActivePermissionAttribute</para>
    /// </summary>
    public static class ActiveRoleEngine
    {
        #region Configuration

        /// <summary>
        /// Restore user session data after session timeout but the user is still authenticated
        /// </summary>
        internal static Func<string, bool> RestoreUserSessionHandler { get; set; }

        /// <summary>
        /// Process unauthorized request
        /// </summary>
        internal static Action<AuthorizationContext, AuthorizeResult> ProcessUnauthorizedRequest { get; set; }

        ///// <summary>
        ///// Define the handler of Unauthorized Error
        ///// <para>When Unauthorized Error occurs, the handler will be executed and display the custom error message to user, and client Url is NOT redirected</para>
        ///// </summary>
        //internal static UnauthorizedHandlerModel UnauthorizedHandlerModel { get; set; }

        #endregion Configuration

        #region ConfigRoleEngine

        /// <summary>
        /// Configurations the role engine.
        /// </summary>
        /// <param name="restoreUserSessionHandler">The restore user session handler</param>
        /// <param name="processUnauthorizedRequest">The process unauthorized request</param>
        public static void ConfigRoleEngine(
            //List<IDbPermission> systemPermissions,
            Func<string, bool> restoreUserSessionHandler,
            Action<AuthorizationContext, AuthorizeResult> processUnauthorizedRequest)
        {
            //if (unauthorizedHandlerModel != null)
            //{
            //    if (unauthorizedHandlerModel.Controller.IsNullOrWhiteSpace())
            //        throw new ArgumentNullException("UnauthorizedHandlerModel.Controller is not defined");

            //    if (unauthorizedHandlerModel.Action.IsNullOrWhiteSpace())
            //        throw new ArgumentNullException("UnauthorizedHandlerModel.Action is not defined");

            //    ActiveRoleEngine.UnauthorizedHandlerModel = unauthorizedHandlerModel;
            //}

            //if (systemPermissions == null)
            //    throw new ArgumentNullException(nameof(systemPermissions));

            ActiveRoleEngine.RestoreUserSessionHandler = restoreUserSessionHandler;
            ActiveRoleEngine.ProcessUnauthorizedRequest = processUnauthorizedRequest;

        }

        #endregion ConfigRoleEngine

        #region PermissionDictionaryCache

        // https://www.ryadel.com/en/asp-net-mvc-fix-ambiguous-action-methods-errors-multiple-action-methods-action-name-c-sharp-core/
        // Action can have same name

        private static Dictionary<object, PermissionModel> _permissionDictionaryCache;

        /// <summary>
        /// Cache the permission for each controller and action, through the ControllerType and MethodInfo
        /// <para>Include both non-superadmin and superadmin permission</para>
        /// </summary>
        /// <value>
        /// The permission dictionary cache
        /// </value>
        public static Dictionary<object, PermissionModel> PermissionDictionaryCache
        {
            get
            {
                #region Initialize

                if (_permissionDictionaryCache == null)
                    InitPermissionCache();

                #endregion Initialize

                return _permissionDictionaryCache;

            }
        }

        #endregion PermissionDictionaryCache

        #region NonSuperAdminPermissions

        private static List<PermissionModel> _nonSuperAdminPermissions;

        /// <summary>
        /// Defined permissions for Non-SuperAdmin
        /// <para>Application should have all permissions here</para>
        /// </summary>
        /// <value>
        /// The non super admin permissions
        /// </value>
        public static List<PermissionModel> NonSuperAdminPermissions
        {
            get => _nonSuperAdminPermissions ??
                (_nonSuperAdminPermissions = PermissionDictionaryCache.Values
                    .Where(p => p.PermissionType != PermissionType.SuperAdmin &&
                                p.PermissionType != PermissionType.Authorized &&
                                p.PermissionType != PermissionType.AuthorizedInternal &&
                                p.PermissionType != PermissionType.AuthorizedExternal)
                    .ToList());

            set => _nonSuperAdminPermissions = value;
        }

        #endregion NonSuperAdminPermissions

        #region SuperAdminPermissions

        private static List<PermissionModel> _superAdminPermissions;

        /// <summary>
        /// Defined permissions for Super Admin
        /// </summary>
        /// <value>
        /// The super admin permissions
        /// </value>
        public static List<PermissionModel> SuperAdminPermissions
        {
            get => _superAdminPermissions ??
                (_superAdminPermissions = PermissionDictionaryCache.Values
                    .Where(p => p.PermissionType == PermissionType.SuperAdmin).ToList());

            set => _superAdminPermissions = value;
        }

        #endregion SuperAdminPermissions

        #region InitPermissionCache

        /// <summary>
        /// Initializes the permission cache.
        /// </summary>
        private static void InitPermissionCache()
        {
            _permissionDictionaryCache = new Dictionary<object, PermissionModel>();

            // all controller types
            IEnumerable<Type> _controllerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type != null
                    && type.IsPublic
                    && typeof(IController).IsAssignableFrom(type) // note: can't use t is IController
                    && !type.IsAbstract);

            foreach (Type controllerType in _controllerTypes)
            {
                // if controller has AllowAnonymousAttribute
                // then ignore it and its action completely (also skipped in ActivePermissionAttribute)
                if (controllerType.GetCustomAttribute<AllowAnonymousAttribute>() != null)
                    continue;

                ActivePermissionAttribute controllerAttribute = controllerType.GetCustomAttribute<ActivePermissionAttribute>(true);

                // Get permission from Controller decorated with ActivePermissionAttribute
                if (controllerAttribute != null)
                {
                    PermissionModel model = new PermissionModel(controllerAttribute, controllerType);
                    AppendPermissionToCache(controllerType, model);
                }

                // Get permission from Action decorated with ActivePermissionAttribute
                controllerType
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(method => method.ReturnType.IsAssignableFrom(typeof(ActionResult)))
                    .ForEach(methodInfo =>
                    {
                        // ignore the action marked as 
                        // - ChildActionOnlyAttribute (always inherit permission from parent action)
                        // - NonActionAttribute
                        // - AllowAnonymousAttribute
                        if (Attribute.IsDefined(methodInfo, typeof(ChildActionOnlyAttribute)) ||
                        Attribute.IsDefined(methodInfo, typeof(NonActionAttribute)) ||
                        Attribute.IsDefined(methodInfo, typeof(AllowAnonymousAttribute)))
                            return;

                        ActivePermissionAttribute actionAttribute = methodInfo.GetCustomAttribute<ActivePermissionAttribute>();

                        if (actionAttribute != null)
                        {
                            PermissionModel model = new PermissionModel(actionAttribute, controllerAttribute, controllerType, methodInfo);
                            AppendPermissionToCache(methodInfo, model);
                        }
                    });
            }
        }

        #endregion InitPermissionCache

        #region AppendPermissionToCache

        /// <summary>
        /// Appends the permission to cache.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="model">The model</param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void AppendPermissionToCache(object key, PermissionModel model)
        {
            KeyValuePair<object, PermissionModel> current = _permissionDictionaryCache.FirstOrDefault(item => item.Value.PermissionId.EqualsIgnoreCase(model.PermissionId));

            // if not found, current = new KeyValuePair();
            if (current.Key == null || current.Value == null)
            {
                _permissionDictionaryCache.Add(key, model);
                return;
            }

            // PermissionModel is existed

            // validate it
            if (current.Value.PermissionType != model.PermissionType)
            {
                string currentType = ActiveRoleEngineHelper.GetDebugNamespace(current.Key);
                string newType = ActiveRoleEngineHelper.GetDebugNamespace(key);

                // same PermissionId but defined different PermissionType
                throw new InvalidOperationException($"PermissionId '{model.PermissionId}' is already defined in '{currentType}' with PermissionType = '{current.Value.PermissionType}'. Cannot declare different PermissionType = '{model.PermissionType}' in '{newType}'");
            }

            // update the group & description value
            if (current.Value.Group.IsNullOrWhiteSpace() && model.Group.IsNotNullOrWhiteSpace())
                current.Value.Group = model.Group;

            if (current.Value.Description.IsNullOrWhiteSpace() && model.Description.IsNotNullOrWhiteSpace())
                current.Value.Description = model.Description;

            _permissionDictionaryCache.Add(key, current.Value);
        }

        #endregion AppendPermissionToCache

        #region GetPermissionById

        /// <summary>
        /// Gets the permission by identifier.
        /// </summary>
        /// <param name="permissionId">The permission identifier</param>
        /// <returns></returns>
        internal static PermissionModel GetPermissionById(string permissionId)
        {
            PermissionModel permission = ActiveRoleEngine.PermissionDictionaryCache
                .Where(p => p.Value.PermissionId.EqualsIgnoreCase(permissionId))
                .Select(p => p.Value).FirstOrDefault();

            return permission;
        }

        #endregion GetPermissionById

        #region IsPermissionDefined

        /// <summary>
        /// Check if the permissionId is defined in the source code
        /// </summary>
        /// <param name="permissionId">The permission identifier</param>
        /// <returns>
        ///   <c>true</c> if [is permission defined] [the specified permission identifier]; otherwise, <c>false</c>
        /// </returns>
        public static bool IsPermissionDefined(string permissionId)
        {
            if (permissionId.IsNullOrEmpty())
                return true;

            PermissionModel permission = GetPermissionById(permissionId);

            return permission != null;
        }

        #endregion IsPermissionDefined

    }
}
