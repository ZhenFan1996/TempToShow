using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class AllSolutionsViewModel
    {
        public List<Solution> Solutions { get; set; }
        public Challenge CurrChallenge { get; set; }
        public String CurrChallengeId { get; set; }

        public String CurrSolutionId { get; set; }

        [RegularExpression(@"^([1-9][0-9]*)$")]
        public int Point { get; set; }
    }
}
