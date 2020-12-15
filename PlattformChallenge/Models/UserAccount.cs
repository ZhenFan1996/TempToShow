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
        public string Email { get; set; }

        public string Passwort { get; set; }

        public AccountTyp AccountTyp { get; set; }
    }   
}
