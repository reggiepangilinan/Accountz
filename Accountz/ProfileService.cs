using Accountz.Domain;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Accountz
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<UserAccount> userManager;

        public ProfileService(UserManager<UserAccount> userManager)
        {
            this.userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await userManager.GetUserAsync(context.Subject);

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Email, user.Email),
                new Claim(JwtClaimTypes.EmailVerified, user.EmailConfirmed.ToString(), ClaimValueTypes.Boolean),
                !string.IsNullOrEmpty(user.PhoneNumber) ?  new Claim(JwtClaimTypes.PhoneNumber, user.PhoneNumber) : null,
                new Claim(JwtClaimTypes.PhoneNumberVerified, user.PhoneNumberConfirmed.ToString(), ClaimValueTypes.Boolean),
            };

            context.IssuedClaims.AddRange(claims.Where(x => x != null));
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await userManager.GetUserAsync(context.Subject);
            context.IsActive = (user != null);
        }
    }
}
