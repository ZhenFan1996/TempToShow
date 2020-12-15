using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class Solution
    {
        [Key]
        public int S_Id { get; set; }
        [Required]
        public string URL { get; set; }
        [Required]
        public StatusEnum Status { get; set; }
        [Required]
        public DateTime Submit_Date { get; set; }
        
        public int Point { get; set; }

        public Participation Participation { get; set; }
    }
}
