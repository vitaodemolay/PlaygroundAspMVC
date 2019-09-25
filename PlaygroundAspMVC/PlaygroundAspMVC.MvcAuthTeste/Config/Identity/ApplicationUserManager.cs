using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace PlaygroundAspMVC.MvcAuthTeste.Config.Identity
{
    internal class SuccessIdentityResult: IdentityResult
    {
        public SuccessIdentityResult(bool success = true)
           : base(success)
        {

        }
    }


    public class ApplicationUserManager : UserManager<ApplicationUser>
    {

        private readonly IUserStore<ApplicationUser> _store;
        public ApplicationUserManager(IUserStore<ApplicationUser> store) : base(store)
        {
            _store = store;
        }


        public override async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            user.password = password;
            await _store.CreateAsync(user);

            return new SuccessIdentityResult();
        }

        public override async Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            await _store.CreateAsync(user);

            return new SuccessIdentityResult();
        }


        public override Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return _store.FindByNameAsync(email);
        }

    }
}