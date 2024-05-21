using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;

namespace Identity.API.Configuration
{
    //Giúp define những dữ liệU ban đầu cho Identity
    //định nghĩa 1 list resource mà ta muốn quản lí (Resource API và Identity Resource)
    public class Config
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
            return new[] 
            {
                new ApiResource
                {
                    Name = "exam_api",
                    DisplayName = "Exam API"
                }
            };
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new[]
            {
                new ApiScope("full_access"), 
            };
        }

        public static IEnumerable<Client> GetClients(Dictionary<string, string> clientUrls)
        {
            return new[]
            {
                new Client
                {
                    ClientId = "exam_web_app",
                    ClientName = "Exam Web App Client",
                    ClientSecrets = new List<Secret>
                        {
                            new Secret("secret".Sha256())
                        },
                    ClientUri = $"{clientUrls["ExamWebApp"]}", // public uri of the client
                    AllowedCorsOrigins = { clientUrls["ExamWebApp"] },
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowAccessTokensViaBrowser = false,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris = new List<string>
                        {
                            $"{clientUrls["ExamWebApp"]}/authentication/login-callback"
                        },
                    PostLogoutRedirectUris = new List<string>
                        {
                            $"{clientUrls["ExamWebApp"]}/authentication/logout-callback"
                        },
                    AllowedScopes = new List<string>
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.OfflineAccess,
                            "full_access",
                        },
                    AccessTokenLifetime = 60 * 60 * 2, // 2 hours
                    IdentityTokenLifetime = 60 * 60 * 2, // 2 hours
                    RequireClientSecret = true, // !Important for authorization

                },

                new Client
                {
                    ClientId = "exam_web_admin",
                    ClientName = "Exam Web Admin Client",
                    ClientSecrets = new List<Secret>
                        {
                            new Secret("secret".Sha256())
                        },
                    ClientUri = $"{clientUrls["ExamWebAdmin"]}", // public uri of the client
                    AllowedCorsOrigins = { clientUrls["ExamWebAdmin"] },
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowAccessTokensViaBrowser = false,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris = new List<string>
                        {
                            $"{clientUrls["ExamWebAdmin"]}/authentication/login-callback"
                        },
                    PostLogoutRedirectUris = new List<string>
                        {
                            $"{clientUrls["ExamWebAdmin"]}/authentication/logout-callback"
                        },
                    AllowedScopes = new List<string>
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.OfflineAccess,
                            "full_access",
                        },
                    AccessTokenLifetime = 60 * 60 * 2, // 2 hours
                    IdentityTokenLifetime = 60 * 60 * 2, // 2 hours
                    RequireClientSecret = true, // !Important for authorization

                },
                
                new Client
                {
                    ClientId = "exam_api_swaggerui",
                    ClientName = "Exam API Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{clientUrls["ExamWebApi"]}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { $"{clientUrls["ExamWebApi"]}/swagger/" },

                    AllowedScopes =
                        {
                            "full_access",
                        },
                }
            };
        }
    }
}