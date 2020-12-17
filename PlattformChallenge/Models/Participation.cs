using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class Participation
    {

        public int C_Id { get; set; }

        public int P_Id { get; set; }

        public Challenge Challenge { get; set; }

        public Programmer Programmer { get; set; }

        public Solution Solution { get; set; }
    }
}
