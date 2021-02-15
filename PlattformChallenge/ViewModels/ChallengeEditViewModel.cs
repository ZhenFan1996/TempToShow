using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class ChallengeEditViewModel
    {
       
        public Challenge Challenge { get; set; }
        public List<Language> Languages { get; set; }
        public bool[] IsSelected { get; set; }
        public bool AllowEditDate { get; set; }
    }
}
