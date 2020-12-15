using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class UserAccount
    {
        [Key]
        public int User_Id { get; set; }
        
        [DataType(DataType.EmailAddress)]
        [Required]
        
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public AccountTyp AccountTyp { get; set; }
    }


}
