using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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

        Task<TokenResponse> Login(CustomerRegistrationRequest customer);
    }

    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly AppIdentityDbContext _context;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;

        public UserRepository(UserManager<AppUser> userManager,RoleManager<AppRole> roleManager,
            AppIdentityDbContext context, SignInManager<AppUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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
                await _userManager.AddToRoleAsync(user, "Agent");
                return true;
            }

            return false;
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

                    var token = await GenerateToken(user);
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

        private async Task<JwtSecurityToken> GenerateToken(AppUser user)
        {

            var userRoles = await _userManager.GetRolesAsync(user);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var nowUtc = DateTime.Now.ToUniversalTime();
            var expires = nowUtc.AddMinutes(double.Parse(_configuration["Tokens:Lifetime"])).ToUniversalTime();
            IdentityOptions _options = new IdentityOptions();
            var claims = new List<Claim>
            {
                
                new Claim(_options.ClaimsIdentity.UserIdClaimType, user.Id.ToString()),
                new Claim(_options.ClaimsIdentity.UserNameClaimType, user.UserName)
            };
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await _roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (Claim roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim);
                    }
                }
            }
            var token = new JwtSecurityToken(
                _configuration["Tokens:Issuer"],
                _configuration["Tokens:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds);

            return token;
        }
    }


}
