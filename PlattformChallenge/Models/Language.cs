using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class Language
    {
        public int Language_Id;

        public string DevelopmentLanguage { get; set; }

        public List<LanguageChallenge> LanguageChallenges { get; set; }
    }
}
