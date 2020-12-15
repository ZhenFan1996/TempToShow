using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PlattformChallenge.Models
{
    public class Challenge
    {
        
        [Required]
        public int Bonus { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string  Content  { get; set; }
        [Required]
        public DateTime Release_Date { get; set; }
        [Required]
        public int Max_Participant { get; set; }

        public int Winner { get; set; }

        public int Best_Solution { get; set; }
        [Required]
        public int Com_ID { get; set; }

        public List<Participation> Participations { get; set; }

        public List<LanguageChallenge> LanguageChallenges { get; set; }

        public Company Company { get; set; }

    }

    

}
