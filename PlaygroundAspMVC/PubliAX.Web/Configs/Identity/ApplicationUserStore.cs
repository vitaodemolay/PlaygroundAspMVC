using Microsoft.AspNet.Identity;
using PubliAX.Domain.DTO;
using System;
using System.Threading.Tasks;
using VMRCPACK.Infrastructure.Interfaces.Dapper;

namespace PubliAX.Web.Configs.Identity
{
    public class ApplicationUserStore : IUserStore<ApplicationUser>, IDisposable
    {

        private IDapper<UserDto> UserRepository { get; set; }
        private bool isDisposed { get; set; }

        public ApplicationUserStore(IDapper<UserDto> UserRepository)
        {
            this.UserRepository = UserRepository;
            isDisposed = false;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                this.UserRepository = null;
            }
        }

        public async Task<ApplicationUser> FindByIdAsync(string userId)
        {
            var guidUserId = Guid.Parse(userId);
            var user = this.UserRepository.Find(f => f.userId == guidUserId);

            if (user != null)
                return new ApplicationUser(user);

            return null;
        }

        public async Task<ApplicationUser> FindByNameAsync(string userName)
        {
            var user = this.UserRepository.Find(f => f.login == userName);

            if (user != null)
                return new ApplicationUser(user);

            return null;
        }




        public Task CreateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }
    }
}