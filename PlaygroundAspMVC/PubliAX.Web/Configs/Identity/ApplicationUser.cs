using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using PubliAX.Domain.DTO;

namespace PubliAX.Web.Configs.Identity
{
    public class ApplicationUser : IUser<string>
    {
        public string Id => baseObject.userId.ToString();

        public string UserName { get => baseObject.login; set => baseObject.login = value; }


        private UserDto baseObject { get; set; }


        public void SetBaseObject(UserDto user)
        {
            baseObject = user;
        }

        public ApplicationUser()
        {

        }

        public ApplicationUser(UserDto user)
        {
            baseObject = user;
        }

        internal Task<ClaimsIdentity> GenerateUserIdentityAsync()
        {
            return Task.FromResult(new ClaimsIdentity(GetClaims()));
        }

        private IEnumerable<Claim> GetClaims()
        {
            yield return new Claim("name", baseObject.name);
            yield return new Claim("role", baseObject.role);
            yield return new Claim("login", baseObject.login);
            yield return new Claim("userId", baseObject.userId.ToString());
            yield return new Claim("email", baseObject.email);
            yield return new Claim("celphone", baseObject.celphone);
        }
    }
}