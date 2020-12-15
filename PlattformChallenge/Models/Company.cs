using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class Company : UserAccount
    {
       
        [Required]
        public string Name { get; set; }
        [Required]
        public bool IsActiv { get; set; }

        public string Logo { get; set; }

        public List<Challenge> Challenges { get; set; }
    }
}
