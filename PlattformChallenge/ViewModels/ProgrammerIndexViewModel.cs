using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class ProgrammerIndexViewModel
    {

        public List<Challenge> Challenges { get; set; }

        public PlatformUser Programmer { get; set; }
    }
}
