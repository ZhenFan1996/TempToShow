using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class Programmer :UserAccount
    {

        public int P_Id { get; set; }

        public string Firstname { get; set; }

        public string Surname { get; set; }
    }
}
