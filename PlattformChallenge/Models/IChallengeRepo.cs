using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public interface IChallengeRepo
    {
        Challenge GetChallengeById(string Id);
        IEnumerable<Challenge> GetAllChallenges();

    }
}
