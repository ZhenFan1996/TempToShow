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

        [Key]
        public string C_Id { get; set; }

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

        [Required]
        public string Com_ID { get; set; }

        public List<Participation> Participations { get; set; }

        public List<LanguageChallenge> LanguageChallenges { get; set; }

        public PlatformUser Company { get; set; }

        public string Winner_Id { get; set; }

        public string Best_Solution_Id { get; set; }

    }

    

}
