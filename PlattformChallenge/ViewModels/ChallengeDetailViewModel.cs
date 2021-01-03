using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class ChallengeDetailViewModel
    {
        public string C_Id { get; set; }

        public int Bonus { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        public DateTime Release_Date { get; set; }
        [Required]
        [Display(Name = "Quota")]
        public int Max_Participant { get; set; }

        [Required]
        public string Com_ID { get; set; }

        public PlatformUser Company { get; set; }
        [Display(Name = "Languages")]
        public List<Language> Languages { get; set; }

        [Display(Name = "Winner")]
        public string Winner_Id { get; set; }

        [Display(Name = "Best Solution")]
        public string Best_Solution_Id { get; set; }
    }
}
