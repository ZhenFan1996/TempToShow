using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Models
{
    public class SQLChallengeRepo : IChallengeRepo
    {
        private readonly AppDbContext _context;
        public SQLChallengeRepo(AppDbContext context)
        {
            _context = context;
        }
        public IEnumerable<Challenge> GetAllChallenges()
        {
            return _context.Challenges;
        }

        public Challenge GetChallengeById(string Id)
        {
            return _context.Challenges.Find(Id);
        }
    }
}
