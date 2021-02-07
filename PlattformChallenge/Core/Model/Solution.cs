using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
namespace PlattformChallenge.Core.Model

{
    public class Solution
    {
        [Key]
        [Display(Name = "Solution ID")]
        public string S_Id { get; set; }
        [Required]
        public string URL { get; set; }
        [Required]
        public StatusEnum Status { get; set; }
        [Required]
        [Display(Name = "Submit Date")]
        public DateTime Submit_Date { get; set; }

        public string FileName { get; set; }

        public int? Point { get; set; }     

        public Participation Participation { get; set; }
    }
}
