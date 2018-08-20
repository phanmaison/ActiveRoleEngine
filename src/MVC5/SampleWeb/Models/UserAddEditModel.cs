using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleWeb.Models
{
    public class UserAddEditModel
    {
        public Guid? UserId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Username { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }
        public bool IsSuperAdmin { get; set; }

        public List<Guid> Roles { get; set; }
    }
}
