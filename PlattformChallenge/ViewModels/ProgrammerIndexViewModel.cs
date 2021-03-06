using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class ProgrammerIndexViewModel
    {

        public List<Challenge> Challenges { get; set; }

        public PlatformUser Programmer { get; set; }

        public List<Participation> Participations { get; set; }

        public string LogoPath { get; set; }

        [Phone]
        public string Phone { get; set; }

        public int InProgress { get; set; }

        public int Completet { get; set; }
    }
    }

