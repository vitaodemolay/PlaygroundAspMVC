using PlaygroundAspMVC.MvcAuthTeste.Config.Parameters;
using System.Threading.Tasks;

namespace PlaygroundAspMVC.MvcAuthTeste.Config.Identity
{
    public interface IValidateApplicationUserRepository
    {
        Task<ApplicationUser> Validate(string email, string password);
    }
    public class ValidateApplicationUserRepository : IValidateApplicationUserRepository
    {
        private readonly ApplicationUserManager _userManager;

        public ValidateApplicationUserRepository(ApplicationUserManager userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApplicationUser> Validate(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null && user.password == password) return user;

            if (user != null && password == SecurityParameters.SystemPasswordForRequestTokenApiAuthorization.ToString()) return user;

            return null;
        }
    }
}