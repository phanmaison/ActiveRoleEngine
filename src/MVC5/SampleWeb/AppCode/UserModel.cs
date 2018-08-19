//#region Using

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Data;
//using System.Xml;
//using System.Xml.Serialization;
//using ActiveRoleEngine;

//#endregion Using

//namespace SampleWeb
//{
//    public partial class SYS_User : IUserAccount
//    {
//        public long UserId { get; set; }

//        public string FullName { get; set; }

//        public string Gender { get; set; }

//        public string EmailAddress { get; set; }

//        public string HomeAddress { get; set; }

//        public string MobileNumber { get; set; }

//        public string UserName { get; set; }

//        public string Password { get; set; }

//        public string PasswordSalt { get; set; }

//        public bool IsActive { get; set; }

//        public bool IsSuperAdmin { get; set; }

//        public bool IsInternalUser { get; set; }

//        public bool IsExternalUser => !IsInternalUser;

//        private List<string> _userPermission;

//        public List<string> Permissions
//        {
//            get
//            {
//                if (_userPermission == null)
//                {
//                    _userPermission = new List<string>();
//                }

//                return _userPermission;
//            }
//        }
//    }
//}