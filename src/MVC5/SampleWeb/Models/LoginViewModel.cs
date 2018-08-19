using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SampleWeb.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
