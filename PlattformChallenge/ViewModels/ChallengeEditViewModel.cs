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
        //[Key]
        //public string C_Id { get; set; }
        //public string Title { get; set; }
        //[RegularExpression(@"^([1-9][0-9]*)$")]
        //public int Bonus { get; set; }
        //public string Content { get; set; }

        //[RegularExpression(@"^([1-9][0-9]*)$")]
        //public int Max_Participant { get; set; }
        //[Display(Name = "Winner")]
        //public string Winner_Id { get; set; }

        //[Display(Name = "Best Solution")]
        //public string Best_Solution_Id { get; set; }
        public Challenge Challenge { get; set; }
        public List<Language> Languages { get; set; }

        public bool[] IsSelected { get; set; }
    }
}
