using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Solution.DLL.Identity.DataModel;

namespace Solution.DLL.Identity.DBContext
{
    public class AppIdentityDbContext : IdentityDbContext<AppUser>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<RefreshToken>()
                .HasAlternateKey(c => c.UserId)
                .HasName("refreshToken_UserId");

            builder.Entity<RefreshToken>()
                .HasAlternateKey(c => c.Token)
                .HasName("refreshToken_Token");
            base.OnModelCreating(builder);
            builder.Entity<AppRole>().HasData(
                new AppRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN",                   
                },
                new AppRole
                {
                    Id = "2",
                    Name = "Customer",
                    NormalizedName = "CUSTOMER",                   
                },
                new AppRole
                {
                    Id = "3",
                    Name = "Agent",
                    NormalizedName = "AGENT",
                }
            );
        }


    }
}
