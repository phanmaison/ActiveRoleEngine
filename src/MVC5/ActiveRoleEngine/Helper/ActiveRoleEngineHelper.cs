using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ActiveRoleEngine
{
    /// <summary>
    /// Helper for Role Engine
    /// </summary>
    public static class ActiveRoleEngineHelper
    {
        #region GetWebEntryAssembly

        // https://jacstech.wordpress.com/2013/09/05/get-the-executing-assembly-of-a-web-app-referencing-a-class-library/
        /// <summary>
        /// Gets the web entry assembly.
        /// </summary>
        /// <returns></returns>
        internal static Assembly GetWebEntryAssembly()
        {
            if (HttpContext.Current == null ||
                HttpContext.Current.ApplicationInstance == null)
            {
                return null;
            }

            Type type = HttpContext.Current.ApplicationInstance.GetType();
            while (type != null && type.Namespace == "ASP")
            {
                type = type.BaseType;
            }

            return type?.Assembly;
        }

        #endregion GetWebEntryAssembly

        #region AllAreaTypes

        private static IEnumerable<Type> _allAreaTypes;

        /// <summary>
        /// Gets all area types
        /// </summary>
        /// <value>
        /// All area types
        /// </value>
        internal static IEnumerable<Type> AllAreaTypes
        {
            get
            {
                if (_allAreaTypes == null)
                {
                    Assembly assembly = GetWebEntryAssembly();
                    _allAreaTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(AreaRegistration)));
                }

                return _allAreaTypes;
            }
        }

        #endregion AllAreaTypes

        #region GetControllerArea

        private static readonly Dictionary<string, string> _controllerAreaDictionary = new Dictionary<string, string>();

        /// <summary>
        /// Get the area name from controller type
        /// </summary>
        /// <param name="controllerType">Type of the controller.</param>
        /// <returns></returns>
        public static string GetControllerArea(Type controllerType)
        {
            if (controllerType == null)
                throw new ArgumentNullException(nameof(controllerType));

            if (_controllerAreaDictionary.ContainsKey(controllerType.Namespace))
                return _controllerAreaDictionary[controllerType.Namespace];

            foreach (Type areaType in AllAreaTypes)
            {
                if (!controllerType.Namespace.StartsWith(areaType.Namespace)) continue;

                AreaRegistration area = (AreaRegistration)Activator.CreateInstance(areaType);

                _controllerAreaDictionary.Add(controllerType.Namespace, area.AreaName);
                return area.AreaName;
            }

            _controllerAreaDictionary.Add(controllerType.Namespace, string.Empty);
            return string.Empty;
        }

        #endregion GetControllerArea

        #region GetControllerName

        /// <summary>
        /// Get the controller name without 'Controller' suffix
        /// </summary>
        /// <param name="controllerType">Type of the controller.</param>
        /// <returns></returns>
        public static string GetControllerName(Type controllerType)
        {
            return controllerType.Name.TrimEnd("Controller");
        }

        #endregion GetControllerName

        #region GetPermissionId

        /// <summary>
        /// Gets the permission identifier
        /// </summary>
        /// <param name="permission">The permission</param>
        /// <param name="area">The area</param>
        /// <param name="controller">The controller</param>
        /// <param name="action">The action</param>
        /// <returns></returns>
        public static string GetPermissionId(string permission, string area, string controller, string action)
        {
            /*
             * Ouput:
             * area/controller/action
             * area/controller
             * controller
             * controller/area
             * 
             * */

            if (permission.IsNotNullOrEmpty())
                return permission.TrimSafe();

            area = area.TrimSafe();
            controller = controller.TrimSafe();
            action = action.TrimSafe();

            if (area.IsNotNullOrEmpty())
                area = area + "/";
            if (action.IsNotNullOrEmpty())
                action = "/" + action;

            return $"{area}{controller}{action}";
        }

        #endregion GetPermissionId

        #region GetMethodInfo

        /// <summary>
        /// Get the MethodInfo from ActionDescriptor
        /// </summary>
        /// <param name="actionDescriptor">The action descriptor</param>
        /// <returns></returns>
        internal static MethodInfo GetMethodInfo(this ActionDescriptor actionDescriptor)
        {
            // https://stackoverflow.com/questions/1972234/how-do-i-get-a-methodinfo-from-an-actionexecutingcontext
            // Refer to System.Web.MVC source code
            // ActionDescriptor is abstract class which is implemented by
            // - ResourceErrorActionDescriptor (private => ignore)
            // - IMethodInfoActionDescriptor
            // + ReflectedActionDescriptor
            // + ReflectedAsyncActionDescriptor
            // + TaskAsyncActionDescriptor

            return ((IMethodInfoActionDescriptor)actionDescriptor)?.MethodInfo;
        }

        #endregion GetMethodInfo

        #region GetDebugNamespace

        /// <summary>
        /// Get namespace of the object
        /// <para>Obj is either type of Type or MethodInfo</para>
        /// </summary>
        /// <param name="obj">The object</param>
        /// <returns></returns>
        internal static string GetDebugNamespace(object obj)
        {
            string currentType;

            switch (obj)
            {
                case MethodInfo methodInfo:
                    currentType = methodInfo.DeclaringType?.FullName + "/" + methodInfo.Name;
                    break;
                case Type _:
                    Type controllerType = (Type)obj;

                    currentType = controllerType.FullName;
                    break;
                default:
                    currentType = $"Type [{obj.ToString()}]";
                    break;
            }

            return currentType;

        }


        #endregion #region GetDebugNamespace

        
            #region GetAcceptVerbs

            /// <summary>
            /// Gets the accept verbs of an action
            /// </summary>
            /// <param name="methodInfo">The action's method information</param>
            /// <returns></returns>
            public static string GetAcceptVerbs(MethodInfo methodInfo)
            {
                if (!methodInfo.IsPublic)
                    return "[None]";

                ActionMethodSelectorAttribute attribute = methodInfo.GetCustomAttributes(typeof(ActionMethodSelectorAttribute), true).Cast<ActionMethodSelectorAttribute>().FirstOrDefault();

                if (attribute == null)
                    return "All";

                if (attribute is AcceptVerbsAttribute)
                {
                    return ((AcceptVerbsAttribute)attribute).Verbs.StringJoin();
                }

                if (attribute is HttpDeleteAttribute)
                    return HttpVerbs.Delete.ToString();

                if (attribute is HttpGetAttribute)
                    return HttpVerbs.Get.ToString();

                if (attribute is HttpHeadAttribute)
                    return HttpVerbs.Head.ToString();

                if (attribute is HttpOptionsAttribute)
                    return HttpVerbs.Options.ToString();

                if (attribute is HttpPatchAttribute)
                    return HttpVerbs.Patch.ToString();

                if (attribute is HttpPostAttribute)
                    return HttpVerbs.Post.ToString();

                if (attribute is HttpPutAttribute)
                    return HttpVerbs.Put.ToString();

                if (attribute is NonActionAttribute)
                    return string.Empty;

                return string.Empty;

            }

            #endregion GetAcceptVerbs

            #region GetMethodsParams

            /// <summary>
            /// Gets the methods parameters.
            /// </summary>
            /// <param name="methodInfo">The method information</param>
            /// <returns></returns>
            public static string GetMethodsParams(MethodInfo methodInfo)
            {
                var paramList = methodInfo.GetParameters();

                if (paramList.Length == 0)
                {
                    return "[No Param]";
                }

                StringBuilder sb = new StringBuilder();

                foreach (ParameterInfo param in paramList)
                {
                    string optional = param.IsOptional ? $" (Optional: {param.DefaultValue})" : "";

                    sb.Append($"{param.ParameterType.Name} {param.Name}{optional}<br>");
                }

                return sb.ToString();
            }

            #endregion GetMethodsParams

    }
}
