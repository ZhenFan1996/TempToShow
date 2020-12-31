using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlattformChallenge.Core.Model;
using PlattformChallenge.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace PlattformChallenge.Data
{
    public static class SeedData
    {
        public static  object UseDataInitializerAsync( this IApplicationBuilder builder)
        {
            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var dbcontext = scope.ServiceProvider.GetService<AppDbContext>();
                var roleMangaer = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

                dbcontext.Database.Migrate();
                if (!dbcontext.Languages.Any()) {
                    dbcontext.Languages.Add(new Language()
                    {
                        Language_Id = "1",
                        DevelopmentLanguage = "Java"

                    });

                    dbcontext.Languages.Add(new Language()
                    {
                        Language_Id = "2",
                        DevelopmentLanguage = "C#"

                    });

                    dbcontext.Languages.Add(new Language()
                    {
                        Language_Id = "3",
                        DevelopmentLanguage = "Python"

                    });

                    dbcontext.Languages.Add(new Language()
                    {
                        Language_Id = "4",
                        DevelopmentLanguage = "C++"

                    });

                    dbcontext.Languages.Add(new Language()
                    {
                        Language_Id = "5",
                        DevelopmentLanguage = "JavaScript"

                    });

                    dbcontext.Languages.Add(new Language()
                    {
                        Language_Id = "6",
                        DevelopmentLanguage = "Go"

                    });
                    dbcontext.Languages.Add(new Language()
                    {
                        Language_Id = "7",
                        DevelopmentLanguage = "Swift"

                    });
                    dbcontext.Languages.Add(new Language()
                    {
                        Language_Id = "8",
                        DevelopmentLanguage = "Other"

                    });

                }

                if (!dbcontext.Roles.Any()) {

                    var IdentityResult1 = roleMangaer.CreateAsync(new IdentityRole("Programmer")).GetAwaiter().GetResult();

                    var IdentityResult2 = roleMangaer.CreateAsync(new IdentityRole("Company")).GetAwaiter().GetResult();



                }
                 dbcontext.SaveChanges();
                return builder;
            }
        }
    }
}

