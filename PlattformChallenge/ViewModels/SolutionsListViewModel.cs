using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class SolutionsListViewModel
    {

        public List<Solution> Solutions { get; set; }

        public Participation Participation { get; set; }
    }
}
