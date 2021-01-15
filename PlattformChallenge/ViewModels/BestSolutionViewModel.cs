using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class BestSolutionViewModel
    {

        public List<Solution> Solutions { get; set; }
        [Required]
        public string C_ID { get; set; }
        public string Best_Name { get; set; }

        public string Best_URL { get; set; }

        public int? Best_Point { get; set; }

    }
}
