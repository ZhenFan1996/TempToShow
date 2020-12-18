using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class Language
    {

        [Key]
        public int Language_Id { get; set; }

        public string DevelopmentLanguage { get; set; }

        public List<LanguageChallenge> LanguageChallenges { get; set; }
    }
}
