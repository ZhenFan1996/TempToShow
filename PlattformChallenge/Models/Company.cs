using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class Company : UserAccount
    {
       

        public string Name { get; set; }

        public bool IsActiv { get; set; }

        public string Logo { get; set; }

        public List<Challenge> Challenges { get; set; }
    }
}
