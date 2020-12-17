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

        protected override void OnModelCreating(ModelBuilder modelBuilder) {

            modelBuilder.Entity<LanguageChallenge>()
                .HasKey(t => new { t.Language_Id, t.C_Id });

            modelBuilder.Entity<UserAccount>()
                .Property(u => u.AccountTyp)
                .HasConversion(
                v => v.ToString(),
                v => (AccountTyp)Enum.Parse(typeof(AccountTyp),v));

            modelBuilder.Entity<Solution>()
            .Property(s => s.Status)
            .HasConversion(
            v => v.ToString(),
            v => (StatusEnum)Enum.Parse(typeof(StatusEnum), v));

            modelBuilder.Entity<UserAccount>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<UserAccount>()
              .HasIndex(u => u.User_Id)
              .IsUnique();

            modelBuilder.Entity<UserAccount>()
                .HasDiscriminator(u => u.AccountTyp)
                .HasValue<UserAccount>(AccountTyp.None)
                .HasValue<Programmer>(AccountTyp.Programmer)
                .HasValue<Company>(AccountTyp.Company);

            modelBuilder.Entity<LanguageChallenge>()
                .HasOne(lc => lc.Language)
                .WithMany(l => l.LanguageChallenges)
                .HasForeignKey(lc => lc.Language_Id);

            modelBuilder.Entity<LanguageChallenge>()
                 .HasOne(lc => lc.Challenge)
                 .WithMany(c => c.LanguageChallenges)
                 .HasForeignKey(lc => lc.C_Id);

            modelBuilder.Entity<Participation>()
                .HasKey(p => new { p.C_Id, p.P_Id });

            modelBuilder.Entity<Participation>()
                .HasOne(pa => pa.Programmer)
                .WithMany(p => p.Participations)
                .HasForeignKey(pa =>pa.P_Id);

            modelBuilder.Entity<Participation>()
                .HasOne(pa => pa.Challenge)
                .WithMany(c => c.Participations)
                .HasForeignKey(pa => pa.C_Id);

            modelBuilder.Entity<Challenge>()
                .HasOne(c => c.Company)
                .WithMany(i =>i.Challenges)
                .HasForeignKey(c => c.Com_ID);
                    
            modelBuilder.Entity<Solution>()
                .HasOne(s => s.Participation)
                .WithOne(p => p.Solution)
                .HasForeignKey<Solution>(s => new { s.P_ID,s.C_ID});

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
