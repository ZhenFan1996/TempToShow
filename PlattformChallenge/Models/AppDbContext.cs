using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PlattformChallenge.Models
{
    public class AppDbContext :DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {

        }

        public DbSet<UserAccount> UserAccounts { get; set; }

        public DbSet<Programmer> Programmers { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Challenge> Challenges { get; set; }

        public DbSet<Participation> Participations { get; set; }

        public DbSet<Language> Languages { get; set; }

        public DbSet<Solution> Solutions { get; set; }
    }
}
