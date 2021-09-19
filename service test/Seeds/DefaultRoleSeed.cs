using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace service_test.Новая_папка.Seeds
{
    public class DefaultRoleSeed
    {
        public static async Task AddDefaultRoleAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole { Name = "User" });
        }
    }
}
