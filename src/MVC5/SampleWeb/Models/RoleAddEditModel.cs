using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleWeb.Models
{
    public class RoleAddEditModel
    {
        public Guid? RoleId { get; set; }
        [Required]
        [MaxLength(100)]
        public string RoleName { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }

        public List<string> Permissions { get; set; }
    }
}
