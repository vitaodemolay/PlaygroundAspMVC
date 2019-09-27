using Microsoft.Owin.Security.OAuth;
using PlaygroundAspMVC.MvcAuthTeste.Config.Identity;
using System.Threading.Tasks;

namespace PlaygroundAspMVC.MvcAuthTeste.Config.Authorization
{
    public class AuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        private readonly IValidateApplicationUserRepository _ValidateUserRepository;

        public AuthorizationServerProvider(IValidateApplicationUserRepository ValidateUserRepository)
        {
            _ValidateUserRepository = ValidateUserRepository;
        }

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }


        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var user = await _ValidateUserRepository.Validate(context.UserName, context.Password);

            if (user == null)
            {
                context.SetError("invalid_grant", "Provided username and password is incorrect");
                return;
            }

            var identity = await user.GenerateUserIdentityAsync(context.Options.AuthenticationType);

            context.Validated(identity);
        }

    }
}