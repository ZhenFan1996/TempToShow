using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class UserAccount
    {
        public string Email { get; set; }
        public string Passwort { get; set; }
        public AccountTyp AccountTyp { get; set; }
    }   
}
