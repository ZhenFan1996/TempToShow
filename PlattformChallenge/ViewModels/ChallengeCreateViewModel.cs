using PlattformChallenge.Core.Interfaces;
using PlattformChallenge.Core.Model;
using PlattformChallenge.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class ChallengeCreateViewModel

    {
        private readonly IRepository<Language> _languages;
        public ChallengeCreateViewModel(IRepository<Language> languages)
        {
            _languages = languages;
             Languages = languages.GetAllListAsync().Result;
        }

        [Key]
        public string C_Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [RegularExpression(@"^([1-9][0-9]*)$")]
        public int Bonus { get; set; }
     
        [Required]
        public string Content { get; set; }
        
        [Required]
        public DateTime Release_Date { get; set; }

        [Required]
 
        public int Max_Participant { get; set; }

        public List<Language> Languages { get; set; } 

        public List<LanguageChallenge> LanguageChallenges { get; set; }

    }
}
