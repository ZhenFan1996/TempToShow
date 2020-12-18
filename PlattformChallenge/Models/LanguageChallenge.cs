using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class LanguageChallenge
    {

        public string Language_Id { get; set; }

        public string C_Id { get; set; }

        public Language Language { get; set; }

        public Challenge Challenge { get; set; }
    }
}
