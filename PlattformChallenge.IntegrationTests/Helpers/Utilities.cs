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
        
        public static void InitializeDbForTests(AppDbContext db, RoleManager<IdentityRole> roleManager)
        {

            var IdentityResult1 = roleManager.CreateAsync(new IdentityRole("Programmer")).GetAwaiter().GetResult();

            var IdentityResult2 = roleManager.CreateAsync(new IdentityRole("Company")).GetAwaiter().GetResult();

            db.Languages.AddRange(GetSeedLanguages());

        }

        public static void ReinitializeDbForTests(AppDbContext db,RoleManager<IdentityRole> roleManager)
        {
            roleManager.DeleteAsync(roleManager.FindByNameAsync("Programmer").Result);
            roleManager.DeleteAsync(roleManager.FindByNameAsync("Company").Result);
            db.Languages.RemoveRange(db.Languages);
            db.LanguageChallenge.RemoveRange(db.LanguageChallenge);
            db.Challenges.RemoveRange(db.Challenges);
            db.Participations.RemoveRange(db.Participations);
            db.Solutions.RemoveRange(db.Solutions);
            db.Users.RemoveRange(db.Users);
            db.UserTokens.RemoveRange(db.UserTokens);
            db.UserRoles.RemoveRange(db.UserRoles);
            db.Roles.RemoveRange(db.Roles);
            InitializeDbForTests(db,roleManager);
        }

        public static List<Language> GetSeedLanguages() {

            return new List<Language>() {
                new Language()
                    {
                        Language_Id = "1",
                        DevelopmentLanguage = "Java"

                    },
                new Language()
                    {
                        Language_Id = "2",
                        DevelopmentLanguage = "C#"
                    },
                new Language()
                    {
                        Language_Id = "3",
                        DevelopmentLanguage = "Python"

                    },
                new Language()
                    {
                        Language_Id = "4",
                        DevelopmentLanguage = "C++"

                    },
                new Language()
                    {
                        Language_Id = "5",
                        DevelopmentLanguage = "JavaScript"

                    },
                new Language()
                    {
                        Language_Id = "6",
                        DevelopmentLanguage = "Go"

                    },
                new Language()
                    {
                        Language_Id = "7",
                        DevelopmentLanguage = "Swift"

                    },
                new Language()
                    {
                        Language_Id = "8",
                        DevelopmentLanguage = "Other"

                    }
            };


        }

        
    }
}
