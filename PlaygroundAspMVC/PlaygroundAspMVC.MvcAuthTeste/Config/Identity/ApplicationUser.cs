using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PlaygroundAspMVC.MvcAuthTeste.Config.Identity
{
    public class ApplicationUser : IUser<string>
    {
        public string Id => userId.ToString();
        public string UserName { get { return name; } set { name = value; } }

        public void SetIdUser(Guid userId)
        {
            this.userId = userId;
        }

        public ApplicationUser()
        {
            this.userId = Guid.NewGuid();
        }

        public ApplicationUser(Guid userId)
        {
            this.userId = userId;
        }


        private Guid userId { get; set; }
        private string name { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public string password { get; set; }

        internal Task<ClaimsIdentity> GenerateUserIdentityAsync()
        {
            return Task.FromResult(new ClaimsIdentity(GetClaims()));
        }

        internal Task<ClaimsIdentity> GenerateUserIdentityAsync(string authenticationType)
        {
            return Task.FromResult(new ClaimsIdentity(GetClaims(), authenticationType));
        }

        private IEnumerable<Claim> GetClaims()
        {
            yield return new Claim("name", name);
            yield return new Claim("role", role);
            yield return new Claim("userId", userId.ToString());
            yield return new Claim("email", email);
        }
    }

}