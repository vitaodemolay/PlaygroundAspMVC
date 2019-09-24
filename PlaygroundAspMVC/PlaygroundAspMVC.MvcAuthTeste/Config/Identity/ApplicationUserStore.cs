using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlaygroundAspMVC.MvcAuthTeste.Config.Identity
{
    public class ApplicationUserStore : IUserStore<ApplicationUser>
    {
        public ApplicationUserStore()
        {
            ApplicationStoreContext.Init();
        }

        public Task CreateAsync(ApplicationUser user)
        {
            if (!ApplicationStoreContext.DataBase.Any(f => f.Id == user.Id))
            {
                ApplicationStoreContext.DataBase.Add(user);
            }
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(ApplicationUser user)
        {
            var _existsUser = await FindByIdAsync(user.Id);

            if (_existsUser != null)
            {
                ApplicationStoreContext.DataBase.Remove(_existsUser);
            }
        }

        public Task<ApplicationUser> FindByIdAsync(string userId)
        {
            return Task.FromResult(ApplicationStoreContext.DataBase.FirstOrDefault(f => f.Id == userId));
        }

        public Task<ApplicationUser> FindByNameAsync(string userName)
        {
            return Task.FromResult(ApplicationStoreContext.DataBase.FirstOrDefault(f => f.UserName == userName));
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            await DeleteAsync(user);

            ApplicationStoreContext.DataBase.Add(user);
        }

        public void Dispose()
        {
            ApplicationStoreContext.DataBase.Clear();
            ApplicationStoreContext.DataBase = null;
        }
    }

    internal static class ApplicationStoreContext
    {
        public static List<ApplicationUser> DataBase;

        public static void Init()
        {
            CreateDb();
        }

        private static void CreateDb()
        {
            DataBase = new List<ApplicationUser>();
            DataBase.Add(new ApplicationUser(Guid.NewGuid())
            {
                email = "jose.anibal@teste.com.br",
                role = "Admin",
                UserName = "Jose Anibal",
                password = "123Mudar"
            });

            DataBase.Add(new ApplicationUser(Guid.NewGuid())
            {
                email = "mario.junior@teste.com.br",
                role = "User",
                UserName = "Mario Cabral Junior",
                password = "123Mudar"
            });

            DataBase.Add(new ApplicationUser(Guid.NewGuid())
            {
                email = "maria.rabelo@teste.com.br",
                role = "User",
                UserName = "Maria Antonia Rabelo",
                password = "123Mudar"
            });
        }
    }

}