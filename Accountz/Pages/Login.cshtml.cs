using Accountz.Domain;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Accountz.Pages
{
    public class LoginModel : PageModel
    {
        private readonly UserManager<UserAccount> userManager;
        private readonly SignInManager<UserAccount> signInManager;
        private readonly IIdentityServerInteractionService interaction;
        private readonly IClientStore clientStore;
        private readonly IAuthenticationSchemeProvider schemeProvider;
        private readonly IEventService events;
        private readonly TestUserStore users;

        [BindProperty]
        [Required]
        public string UsernameEmail { get; set; }

        [BindProperty]
        [Required]
        public string Password { get; set; }

        [FromQuery]
        public string ReturnUrl { get; set; }

        public LoginModel(
            UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            TestUserStore users = null)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.interaction = interaction;
            this.clientStore = clientStore;
            this.schemeProvider = schemeProvider;
            this.events = events;
            this.users = users;
        }

        public IActionResult OnGet()
        {
            if(User.Identity.IsAuthenticated)
                return Redirect("/SecurePageProfile");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(UsernameEmail, Password, true, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var user = await userManager.FindByNameAsync(UsernameEmail);
                    await events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));

                    AuthenticationProperties props = null;

                    //Remember me
                    if (true)
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(30))
                        };
                    await HttpContext.SignInAsync(user.Id, user.UserName, props);

                    if (interaction.IsValidReturnUrl(ReturnUrl) || Url.IsLocalUrl(ReturnUrl))
                        return Redirect(ReturnUrl);
                    return Redirect("/SecurePageProfile");
                }
                //if (users.ValidateCredentials(UsernameEmail, Password))
                //{
                //    var user = users.FindByUsername(UsernameEmail);

                //    await events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.SubjectId, user.Username));

                //    AuthenticationProperties props = null;

                //    //Remember me
                //    if (true)
                //        props = new AuthenticationProperties
                //        {
                //            IsPersistent = true,
                //            ExpiresUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(30))
                //        };
                //    await HttpContext.SignInAsync(user.SubjectId, user.Username, props);

                //    if (interaction.IsValidReturnUrl(ReturnUrl) || Url.IsLocalUrl(ReturnUrl))
                //        return Redirect(ReturnUrl);
                //    return Redirect("/SecurePageProfile");
                //}
                else
                {
                    await events.RaiseAsync(new UserLoginFailureEvent(UsernameEmail, "Invalid credentials"));
                    ModelState.AddModelError("", "Invalid credentials");
                }
            }
            return Page();
        }
    }
}