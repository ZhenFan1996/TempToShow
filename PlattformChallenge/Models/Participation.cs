using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class Participation
    {

        public string C_Id { get; set; }

        public string P_Id { get; set; }

        public string S_Id { get; set; }

        public Challenge Challenge { get; set; }

        public PlatformUser Programmer { get; set; }

        public Solution Solution { get; set; }

    }
}
