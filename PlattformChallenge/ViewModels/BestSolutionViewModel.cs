using PagedList;
using PlattformChallenge.Core.Model;
using System.ComponentModel.DataAnnotations;
namespace PlattformChallenge.ViewModels
{
    public class BestSolutionViewModel
    {

        public IPagedList<Solution> Solutions { get; set; }
        [Required]
        public string C_ID { get; set; }

        public string S_ID { get; set; }
        public string Best_Name { get; set; }

        public string Best_URL { get; set; }

        public int? Best_Point { get; set; }

        public string Winner_ID { get; set; }

    }
}
