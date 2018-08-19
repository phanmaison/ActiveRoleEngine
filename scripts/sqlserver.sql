
CREATE TABLE SysPermission
( 
	PermissionId         nvarchar(255)  NOT NULL ,
	PermissionName       nvarchar(255)  NULL ,
	PermissionGroup      nvarchar(100)  NULL ,
	Description          nvarchar(4000)  NULL 
)
go



ALTER TABLE SysPermission
	ADD  PRIMARY KEY (PermissionId ASC)
go



CREATE TABLE SysRole
( 
	RoleId               uniqueidentifier  NOT NULL ,
	RoleName             nvarchar(100)  NULL ,
	Description          nvarchar(4000)  NULL 
)
go



ALTER TABLE SysRole
	ADD  PRIMARY KEY (RoleId ASC)
go



CREATE TABLE SysRolePermission
( 
	RoleId               uniqueidentifier  NOT NULL ,
	PermissionId         nvarchar(255)  NOT NULL 
)
go



ALTER TABLE SysRolePermission
	ADD  PRIMARY KEY (RoleId ASC,PermissionId ASC)
go



CREATE TABLE SysUser
( 
	UserId               uniqueidentifier  NOT NULL ,
	Username             nvarchar(100)  NULL ,
	Password             nvarchar(100)  NULL ,
	FullName             nvarchar(100)  NULL ,
	IsSuperAdmin         bit  NULL 
)
go



ALTER TABLE SysUser
	ADD  PRIMARY KEY (UserId ASC)
go



CREATE TABLE SysUserPermission
( 
	UserId               uniqueidentifier  NOT NULL ,
	PermissionId         nvarchar(255)  NOT NULL ,
	IsAdd                bit  NULL 
)
go



ALTER TABLE SysUserPermission
	ADD  PRIMARY KEY (UserId ASC,PermissionId ASC)
go



CREATE TABLE SysUserRole
( 
	UserId               uniqueidentifier  NOT NULL ,
	RoleId               uniqueidentifier  NOT NULL 
)
go



ALTER TABLE SysUserRole
	ADD  PRIMARY KEY (UserId ASC,RoleId ASC)
go




ALTER TABLE SysRolePermission
	ADD  FOREIGN KEY (RoleId) REFERENCES SysRole(RoleId)
go




ALTER TABLE SysRolePermission
	ADD  FOREIGN KEY (PermissionId) REFERENCES SysPermission(PermissionId)
go




ALTER TABLE SysUserPermission
	ADD  FOREIGN KEY (UserId) REFERENCES SysUser(UserId)
go




ALTER TABLE SysUserPermission
	ADD  FOREIGN KEY (PermissionId) REFERENCES SysPermission(PermissionId)
go




ALTER TABLE SysUserRole
	ADD  FOREIGN KEY (UserId) REFERENCES SysUser(UserId)
go




ALTER TABLE SysUserRole
	ADD  FOREIGN KEY (RoleId) REFERENCES SysRole(RoleId)
go


