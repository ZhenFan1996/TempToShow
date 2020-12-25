using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Core.Model

{
    public class PlatformUser:IdentityUser
    {
        public string Name { get; set; }

        public string Logo { get; set; }

        public bool IsActiv { get; set; }

        public List<Participation> Participations { get; set; }

        public List<Challenge> Challenges { get; set; }
    }
}
