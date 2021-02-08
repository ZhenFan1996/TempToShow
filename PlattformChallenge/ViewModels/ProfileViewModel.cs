using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class ProfileViewModel
    {
        [EmailAddress]
        public string Email { get; set; }

        public string Name { get; set; }

        public string LogoPath { get; set; }
        
        public string Address { get; set; }

        public string Hobby { get; set; }

        public string Bio { get; set; }
        [Phone]
        public string Phone { get; set; }

        public int TakePartInNummber { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        public DateTime Birthday { get; set; }
    }
}
