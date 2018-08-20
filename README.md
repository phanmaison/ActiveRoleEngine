# Active Role Engine

### Which problem Active Role Engine solve?

I'm not happy with the Authorize attribute which is too simple and difficult to scale  
`[Authorize(Users = "Admin, Manager", Roles = "Manager")]`
  
The roles seem to be fixed and require source code change to add more role.

Active Role Engine aims at a flexible authorization using **permission-based** approach.
- Controller action has its own permission
- Role is defined base on such permission
- User permission can override role permission

So that role can be flexibly defined and scaled.  
Moreover, with Active Role Engine, defining permission is easy like this  
` [ActivePermission]`  
`[ActivePermission(Permission = "CustomPermission", Description = "Custom Permission")]`



### General Requirement

Permission-based access control is not something new, it has existed for decades.

- Permission can be defined for each controller
- Permission can be defined for each controller action
- Permission is inheritable:
    - Controller action can inherit permission from its controller
    - Controller can inherit permission from its base class
- Role is a group of permissions
- User may have many roles
- User inherit permission from roles
- User can override the role permission

### Database Design
![Database Design](/docs/dbdesign.png "")

- SysUser: store user information
- SysPermission: store permissions (optional)
- SysRole: store role information
- SysUserRole: store user's role
- SysRolePermission: store role's permission
- SysUserPermission: override role's permission. Is Add
  - If true: always add the permission to user, 
  - if false: always remove the permission from user

### How To Use

- You can found the script to create database in `scripts` folder. You can freely select your persistence solution
- Decorate controller or controller action with `ActivePermission` attribute
  - By default, the PermissionId is the controller or controller action's name
  - You can override the default PermissionId by providing your own
  - The option `Group` and `Description` is used for displaying purpose only
  - `PermissionType` can be:
    - Permisson (default): user require specific permission to access
    - SuperAdmin: only super admin can access the feature
    - Authenticated: all authenticated users can access
- The entry point is `AuthConfig.ConfigRoleEngine`:
  - `RestoreUserSession`: currently the user permission is stored in Session which has a limitation: session timeout. Hence, `RestoreUserSession` is neccessary for the engine to restore the user's permission
  - `HandleUnauthorizedRequest` (optional): how system handle the unauthorized request. If not provided, the engine will return 403 Forbidden
- The engine does require your extra work to manage user, role, permission


Entire sample is provided in SampleWeb application for your reference. The data is stored in memory hence it will be reset next time you start the application.

### Sample Code
**Super Admin**

    [ActivePermission(PermissionType = PermissionType.SuperAdmin, Group = "Super Admin", Description = "This controller can be accessed by supper admin only")]
    public class SuperAdminController : ActiveControllerBase
    {
        [ActivePermission(PermissionType = PermissionType.SuperAdmin, Description = "This action can be access by super admin only")]
        public ActionResult Index()
        {
            return AuthorizedContent();
        }

        // inherit permission from controller
        public ActionResult Default()
        {
            return AuthorizedContent();
        }
    }
**Inherit**

    [ActivePermission(Group = "Controller Permission", Description = "All actions in the controller have the same permission")]
    public class ControllerPermissionController : ActiveControllerBase
    {
        // inherit permission from controller
        public ActionResult Index()
        {
            return AuthorizedContent();
        }

        // inherit permission from controller
        public ActionResult Action1()
        {
            return AuthorizedContent();
        }
        // inherit permission from controller
        public ActionResult Action2()
        {
            return AuthorizedContent();
        }

        [AllowAnonymous]
        public ActionResult AllowAnonymous()
        {
            return AuthorizedContent();
        }
    }

**Action Permission**

    [ActivePermission(Group = "Action Permission", Description = "Each action has its own permission")]
    public class ActionPermissionController : ActiveControllerBase
    {
        [ActivePermission]
        public ActionResult Index()
        {
            return AuthorizedContent();
        }
        [ActivePermission]
        public ActionResult Action1()
        {
            return AuthorizedContent();
        }
        [ActivePermission]
        public ActionResult Action2()
        {
            return AuthorizedContent();
        }


        [ActivePermission(Permission = "CustomPermissionId", Description = "Action3: PermissionId can be customized")]
        public ActionResult Action3()
        {
            return AuthorizedContent();
        }

        [AllowAnonymous]
        public ActionResult AllowAnonymous()
        {
            return AuthorizedContent();
        }

### Technical Note
- You can skip the authorization with `AllowAnonymousAttribute` or `NonActionAttribute`. `ChildAction` is also skipped
- The sample is using OWIN to simplify the user principle management, you can replace with your own

### Contributor
The first version is developed using ASP.NET MVC5 and will be developed for .NET Core soon.    
I'm looking for contributors to ship the solution to Java, PHP, NodeJs, Go, .. or recommend the existing solution so that I will put the reference here.
**
**