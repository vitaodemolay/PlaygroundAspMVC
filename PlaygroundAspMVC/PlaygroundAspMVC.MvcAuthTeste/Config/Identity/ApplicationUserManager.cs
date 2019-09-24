using Microsoft.AspNet.Identity;

namespace PlaygroundAspMVC.MvcAuthTeste.Config.Identity
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store) : base(store)
        {
        }

    }
}