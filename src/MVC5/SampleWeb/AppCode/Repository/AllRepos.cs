using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SampleWeb.Domain;

namespace SampleWeb.Repository
{
    public class SysUserRepository
    {
        public static List<SysUser> Users { get; } = new List<SysUser>();

        public SysUserRepository()
        {
            
        }

    }
}
