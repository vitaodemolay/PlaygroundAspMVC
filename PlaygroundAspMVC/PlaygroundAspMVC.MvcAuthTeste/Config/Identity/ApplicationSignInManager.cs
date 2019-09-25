using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Threading.Tasks;


namespace PlaygroundAspMVC.MvcAuthTeste.Config.Identity
{
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {

        private readonly ApplicationUserManager _userManager;

        public ApplicationSignInManager(UserManager<ApplicationUser, string> userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
            _userManager = userManager as ApplicationUserManager;
        }


        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync();
        }


        public override async Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            var user = await _userManager.FindByEmailAsync(userName);
            if (user != null && user.password == password)
                return SignInStatus.Success;

            return SignInStatus.Failure;
        }

    }
}