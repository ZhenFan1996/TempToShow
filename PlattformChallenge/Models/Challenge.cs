using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PlattformChallenge.Models
{
    public class Challenge
    {
        [Key]
        public int C_Id { get; set; }

        public int Bonus { get; set; }

        public string Title { get; set; }

        public string  Content  { get; set; }

        public  DateTime Release_Date { get; set; }

        public int Max_Participant { get; set; }

        public int Winner { get; set; }

        public int Best_Solution { get; set; }
        
        public int Com_ID { get; set; }

        public List<Participation> Participations { get; set; }

        public List<Language> Languages { get; set; }

        public Company Company { get; set; }

    }

    

}
