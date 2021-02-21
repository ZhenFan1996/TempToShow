using Microsoft.AspNetCore.Http;
using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class ProfileSettingViewModel
    {
        public PlatformUser User { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public IFormFile Logo { get; set; }

        public string LogoPath { get; set; }

        public string Address { get; set; }

        public string Hobby { get; set; }

        public string Bio { get; set; }

        public string Phone { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        public DateTime Birthday { get; set; } 

    }
}
