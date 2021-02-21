using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PlattformChallenge.Core.Model;
using PlattformChallenge.Infrastructure;

namespace PlattformChallenge.IntegrationTests.Helpers
{

    public static class Utilities
    {
        
        public static void InitializeDbForTests(AppDbContext db)
        {

            db.Challenges.AddRange(GetSeedChallenges());
            db.SaveChanges();
        }

        public static  void InitializeIdentity(UserManager<PlatformUser> um,RoleManager<IdentityRole> rm) {
            var IdentityResult1 = rm.CreateAsync(new IdentityRole("Programmer")).GetAwaiter().GetResult();
            var IdentityResult2 = rm.CreateAsync(new IdentityRole("Company")).GetAwaiter().GetResult();
            var dc = GetSeedUser();
            foreach (var p in dc.Keys) {
               var result = um.CreateAsync(p,"a123456").GetAwaiter().GetResult();
               var token =  um.GenerateEmailConfirmationTokenAsync(p).GetAwaiter().GetResult();
                if (result.Succeeded) {
                    var resultRole =  um.AddToRoleAsync(p,dc.GetValueOrDefault(p)).GetAwaiter().GetResult();
                }
            }

        }

        public static  void ReinitializeDbForTestsAsync(AppDbContext db,UserManager<PlatformUser> um, RoleManager<IdentityRole> rm)
        {

            db.LanguageChallenge.RemoveRange(db.LanguageChallenge);
            db.Challenges.RemoveRange(db.Challenges);
            db.Participations.RemoveRange(db.Participations);
            db.Solutions.RemoveRange(db.Solutions);
            db.Users.RemoveRange(db.Users);
            db.UserTokens.RemoveRange(db.UserTokens);
            db.UserRoles.RemoveRange(db.UserRoles);
            db.UserClaims.RemoveRange(db.UserClaims);
            InitializeDbForTests(db);
            InitializeIdentity(um,rm);
        }

        public static List<Challenge> GetSeedChallenges() {
            var users = GetSeedUser();
            return new List<Challenge>()
            {
                new Challenge(){
                C_Id ="test 1",
                Title ="Test challenge 1",
                Bonus = 10,
                Release_Date = DateTime.Now,
                Content = "Content of test 1",
                Max_Participant = 10,
                Com_ID = "com1"               
                },




            };
        }

        public static Dictionary<PlatformUser,string> GetSeedUser() {

            return new Dictionary<PlatformUser, string>()
            {
                {
                   new PlatformUser(){
                       Id = "pro1",
                       Name ="P",
                       Email = "p@kit.edu",
                       UserName = "p@kit.edu"
                   },"Programmer"
                },
                {
                    new PlatformUser(){
                        Id ="com1",
                        Name ="C",
                        Email = "c@kit.edu",
                        UserName ="c@kit.edu"
                    },"Company"

                }

            };
        }


        }

        
    }

