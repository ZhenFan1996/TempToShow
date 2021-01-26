using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public string Title { get; set; }

        public string Name { get; set; }

        public bool IsVaild { get; set; }

        public IFormFile SolutionFile { get; set; }
    }
}
