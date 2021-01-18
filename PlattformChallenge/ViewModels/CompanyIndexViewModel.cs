using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class CompanyIndexViewModel
    {
        public List<Challenge> Challenges { get; set; }

        public PlatformUser Company { get; set; }
    }
}
