using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Solution.DLL.Identity.DataModel;
using Solution.DLL.Identity.DBContext;
using Solution.DLL.Identity.RequestModel;
using Solution.DLL.Identity.ResponseModel;

namespace Solution.DLL.Identity.Repository
{

    public interface IUserRepository
    {
        Task<bool> Create(CustomerRegistrationRequest customer);

        void SeedData();
        Task<TokenResponse> Login(CustomerRegistrationRequest customer);
    }

    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppIdentityDbContext _context;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;

        public UserRepository(UserManager<AppUser> userManager,
            AppIdentityDbContext context, SignInManager<AppUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _configuration = configuration;
        }


        public async Task<bool> Create(CustomerRegistrationRequest customer)
        {
            var user = new AppUser {UserName = customer.UserName, Email = customer.Email};
            var result = await _userManager.CreateAsync(user, customer.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                return true;
            }

            return false;
        }

        public void SeedData()
        {
            if (!EnumerableExtensions.Any(_context.Roles))
            {
                var roles = new List<String>()
                {
                    "Admin",
                    "Manager",
                    "Customer"
                };

                foreach (var role in roles)
                {
                    var roleToChoose = new IdentityRole(role);
                    _context.Roles.Add(roleToChoose);
                }

                _context.SaveChanges();
            }
        }

        public async Task<TokenResponse> Login(CustomerRegistrationRequest customer)
        {
            var user = await _userManager.FindByEmailAsync(customer.Email);
            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, customer.Password, false);
                if (result.Succeeded)
                {
                    var refreshDbToken = _context.RefreshTokens.FirstOrDefault(x => x.UserId == user.Id);

                    if (refreshDbToken != null)
                    {
                        _context.RefreshTokens.Remove(refreshDbToken);
                        await _context.SaveChangesAsync();
                    }

                    var newRefreshToken = new RefreshToken
                    {
                        UserId = user.Id,
                        Token = Guid.NewGuid().ToString(),
                        IssuedUtc = DateTime.Now.ToUniversalTime(),
                        ExpiresUtc = DateTime.Now.AddMinutes(5)
                    };
                    _context.RefreshTokens.Add(newRefreshToken);
                    await _context.SaveChangesAsync();

                    var token = GenerateToken();
                    var response = new TokenResponse
                    {
                        AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                        RefreshToken = newRefreshToken.Token,
                        UserName = user.UserName,
                        Email = user.Email,
                        TokenExpiration = token.ValidTo
                    };
                    return response;
                }
            }

            throw new Exception("user not found");
        }

        private JwtSecurityToken GenerateToken()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var nowUtc = DateTime.Now.ToUniversalTime();
            var expires = nowUtc.AddMinutes(double.Parse(_configuration["Tokens:Lifetime"])).ToUniversalTime();

            var token = new JwtSecurityToken(
                _configuration["Tokens:Issuer"],
                _configuration["Tokens:Audience"],
                null,
                expires: expires,
                signingCredentials: creds);

            return token;
        }
    }


}
