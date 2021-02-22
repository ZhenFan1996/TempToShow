using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class WonChallengeViewModel
    {
        public List<Challenge> Challenges { get; set; }
        public int SumBonus { get; set; }
    }
}
