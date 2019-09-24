using Microsoft.AspNet.Identity;

namespace PubliAX.Web.Configs.Identity
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store) : base(store)
        {
        }

    }
}