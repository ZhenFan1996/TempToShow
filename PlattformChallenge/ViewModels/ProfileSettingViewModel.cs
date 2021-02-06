using Microsoft.AspNetCore.Http;
using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class ProfileSettingViewModel
    {
        public PlatformUser User { get; set; }

        public string Name { get; set; }

        public IFormFile Logo { get; set; }

        public string LogoPath { get; set; }

        public string Address { get; set; }

        public string Hobby { get; set; }

        public string Bio { get; set; }

        public string Phone { get; set; }

        public DateTime Birthday { get; set; }

    }
}
