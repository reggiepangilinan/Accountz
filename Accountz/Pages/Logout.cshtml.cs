using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Accountz.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly IEventService events;
        private readonly IIdentityServerInteractionService interaction;

        [FromQuery]
        public string LogoutId { get; set; }

        public bool ShowLogoutPrompt { get; set; }

        public bool AutomaticRedirectAfterSignOut { get; set; }

        public string PostLogoutRedirectUri { get; set; }

        public string ClientName { get; set; }

        public string SignOutIframeUrl { get; set; }

        public LogoutModel(IEventService events, IIdentityServerInteractionService interaction)
        {
            this.events = events;
            this.interaction = interaction;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            if (User?.Identity.IsAuthenticated != true)
                return Page();
            
            var context = await interaction.GetLogoutContextAsync(LogoutId);
            if (context?.ShowSignoutPrompt == false)
                // it's safe to automatically sign-out
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await OnPostAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await interaction.GetLogoutContextAsync(LogoutId);

            AutomaticRedirectAfterSignOut = false;

            PostLogoutRedirectUri = logout?.PostLogoutRedirectUri;

            ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName;

            SignOutIframeUrl = logout?.SignOutIFrameUrl;

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await HttpContext.SignOutAsync();
                // raise the logout event
                await events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }
            return Redirect("LoggedOut");
        }
    }
}