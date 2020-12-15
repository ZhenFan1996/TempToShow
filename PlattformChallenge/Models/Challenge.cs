using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class Challenge
    {

        public int C_Id { get; set; }

        public int Bonus { get; set; }

        public string Title { get; set; }

        public string  Content  { get; set; }

        public  DateTime Release_Date { get; set; }

        public int Max_Participant { get; set; }

        public int Winner { get; set; }

        public int Best_Solution { get; set; }
    }

    

}
