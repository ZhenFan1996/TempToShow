using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class LanguageChallenge
    {

        public int Language_Id { get; set; }

        public int C_Id { get; set; }

        public Language Language { get; set; }

        public Challenge Challenge { get; set; }
    }
}
