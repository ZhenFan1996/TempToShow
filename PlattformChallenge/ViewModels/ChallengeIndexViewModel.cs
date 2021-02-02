using PlattformChallenge.Core.Model;
using PlattformChallenge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class ChallengeIndexViewModel
    {
        public List<Language> Languages { get; set; }

        public bool[] IsSelected { get; set; }

        public PaginatedList<Challenge> Challenges { get; set; }

        public int? PageNumber { get; set; }

        public string SortOrder { get; set; }

        public string SearchString { get; set; }
    }
}
