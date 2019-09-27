using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using PlaygroundAspMVC.MvcAuthTeste.Config.Authorization;
using PlaygroundAspMVC.MvcAuthTeste.Config.Identity;
using System;

[assembly: OwinStartup(typeof(PlaygroundAspMVC.MvcAuthTeste.App_Start.Startup))]

namespace PlaygroundAspMVC.MvcAuthTeste.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //Get Instance IValidateApplicationUserRepository
            var ValidateUserRepository = StructuremapMvc.StructureMapDependencyScope.Container.GetInstance<IValidateApplicationUserRepository>();

            // Enable CORS (cross origin resource sharing) for making request using browser from different domains
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            OAuthAuthorizationServerOptions options = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                //The Path For generating the Toekn
                TokenEndpointPath = new PathString("/api/token"),
                //Setting the Token Expired Time (24 hours)
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                //AuthorizationServerProvider class will validate the user credentials
                Provider = new AuthorizationServerProvider(ValidateUserRepository)
            };
            //Token Generations
            app.UseOAuthAuthorizationServer(options);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(20),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync())
                }
            });

        }
    }
}
