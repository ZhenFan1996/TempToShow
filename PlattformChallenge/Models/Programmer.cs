using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class Programmer :UserAccount
    {
    

        [Required]
        public string Firstname { get; set; }

        [Required]
        public string Surname { get; set; }

        public List<Participation> Participations { get; set; }
    }
}
