using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace PlaygroundAspMVC.MvcAuthTeste.Config.Identity
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {

        private readonly IUserStore<ApplicationUser> _store;
        public ApplicationUserManager(IUserStore<ApplicationUser> store) : base(store)
        {
            _store = store;
        }


        public override async Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            await _store.CreateAsync(user);

            return new IdentityResult();
        }


        public override Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return _store.FindByNameAsync(email);
        }

    }
}