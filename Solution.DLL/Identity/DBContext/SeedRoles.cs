using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Solution.DLL.Identity.DBContext
{
   public static class SeedRoles
    {
        public static void GenerateRole(IApplicationBuilder applicationBuilder)
        {
            AppIdentityDbContext context = applicationBuilder.ApplicationServices.GetRequiredService<AppIdentityDbContext>();

            if (!context.Roles.Any())
            {
                var roles = new List<String>()
                {
                    "Admin", "Manager", "Customer"
                };

                foreach (var role in roles)
                {
                    var roleToChoose = new IdentityRole(role);
                    context.Roles.Add(roleToChoose);
                }

                context.SaveChanges();
            }
            

        }
    }
}
