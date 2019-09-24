using System;
using System.Security.Principal;

namespace PlaygroundAspMVC.MvcAuthTeste.Config.Authentication
{
    public interface ICustomPrincipal : IPrincipal
    {
        Guid userId { get; set; }
        string name { get; set; }
        string email { get; set; }
        string role { get; }
    }


    public class CustomPrincipal : ICustomPrincipal
    {
        public IIdentity Identity { get; private set; }
        public Guid userId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string role { get; private set; }

        public bool IsInRole(string role) { return role == this.role; }

        public CustomPrincipal(string name, string role, string email, Guid userId)
        {
            this.Identity = new GenericIdentity(name, role);
            this.role = role;
            this.name = name;
            this.email = email;
            this.userId = userId;
        }
    }

    public class SerializeCustomPrincipal
    {
        public Guid userId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string role { get; set; }
    }
}