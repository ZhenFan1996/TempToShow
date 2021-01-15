using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.ViewModels
{
    public class UploadSolutionViewModel
    {

        public Participation Participation { get; set; }

        public Challenge Challenge { get; set; }

        public PlatformUser Programmer { get; set; }

        public string C_Id { get; set; }

        public string P_Id { get; set; }

        public IFormFile SolutionFile { get; set; }
    }
}
