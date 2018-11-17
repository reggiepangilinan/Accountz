using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Accountz.Pages
{
    public class LoginModel : PageModel
    {
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

        public LoginModel(IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            TestUserStore users = null)
        {
            this.interaction = interaction;
            this.clientStore = clientStore;
            this.schemeProvider = schemeProvider;
            this.events = events;
            this.users = users;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                if (users.ValidateCredentials(UsernameEmail, Password))
                {
                    var user = users.FindByUsername(UsernameEmail);

                    await events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.SubjectId, user.Username));

                    AuthenticationProperties props = null;
                    //Remember me
                    if (true)
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(30))
                        };
                    };
                    await HttpContext.SignInAsync(user.SubjectId, user.Username, props);

                    if (interaction.IsValidReturnUrl(ReturnUrl) || Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                }
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