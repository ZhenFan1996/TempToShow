using System;
using System.Collections.Generic;
using System.Text;
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

        public static void ReinitializeDbForTests(AppDbContext db)
        {

            db.LanguageChallenge.RemoveRange(db.LanguageChallenge);
            db.Challenges.RemoveRange(db.Challenges);
            db.Participations.RemoveRange(db.Participations);
            db.Solutions.RemoveRange(db.Solutions);
            db.Users.RemoveRange(db.Users);
            db.UserTokens.RemoveRange(db.UserTokens);
            db.UserRoles.RemoveRange(db.UserRoles);
            InitializeDbForTests(db);
        }

        public static List<Challenge> GetSeedChallenges() {

            return new List<Challenge>()
            {
                new Challenge(){
                C_Id ="test 1",
                Title ="Test challenge 1",
                Bonus = 10,
                Release_Date = DateTime.Now,
                Content = "Content of test 1",
                Max_Participant = 10,
                Company = new PlatformUser(){
                    Id = "company 1"
                },
                Com_ID = "company 1"
                
                }



            };
        }


        }

        
    }

