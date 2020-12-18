using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
      
        [Required]
        [DataType(DataType.Password)]
        [Compare("Passwird",ErrorMessage = "The passwords are not inconsistent!")]
        public string ConfirmPassword { get; set; }

        public string RoleName { get; set; }
    }
}
