using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accountz
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                new ApiResource("futurestack.api", "FutureStack.API")
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                // SPA client using implicit flow
                new Client
                {
                    ClientId = "futurestack.web",
                    ClientName = "Future Stack Web Client (SPA)",
                    ClientUri = "http://localhost:5050",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RedirectUris =
                    {
                        "http://localhost:5050/callback",
                        "http://localhost:5050/oidc/silent_renew.html"
                    },

                    PostLogoutRedirectUris = { "http://localhost:5050" },
                    AllowedCorsOrigins = { "http://localhost:5050" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "futurestack.api"
                    },
                    AccessTokenLifetime = 3600 //1hr
                }
            };
        }
    }
}
