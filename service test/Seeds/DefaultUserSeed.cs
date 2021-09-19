using Microsoft.AspNetCore.Identity;
using service_test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace service_test.Seeds
{
    public class DefaultUserSeed
    {
        public static async Task DefaultUserAsync(UserManager<User> userManager)
        {
            if(await userManager.FindByNameAsync("Admin") == null)
            {
                var user = new User
                {
                    FirstName = "Admin",
                    UserName = "Admin",
                    Email = "Admin"
                };
                await userManager.CreateAsync(user, "ProjectAdmin");
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
