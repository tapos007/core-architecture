using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using Solution.DLL.Identity.DataModel;
using Solution.DLL.Identity.DBContext;

namespace Solution.DLL.Identity.Repository
{

    public interface IUserRepository
    {
         Task<bool> Create( string userName,
            string password);

         void SeedData();
    }

    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppIdentityDbContext _context;

        public UserRepository(UserManager<AppUser> userManager,AppIdentityDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        public async Task<bool> Create(string userName, string password)
        {
            var user = new AppUser {UserName = userName, Email = userName};
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {

                return true;
            }

            return false;
        }

        public void SeedData()
        {
            if (!_context.Roles.Any())
            {
                var roles = new List<String>()
                {
                    "Admin", "Manager", "Customer"
                };

                foreach (var role in roles)
                {
                    var roleToChoose = new IdentityRole(role);
                    _context.Roles.Add(roleToChoose);
                }

                _context.SaveChanges();
            }
        }
    }
}
