using System;
using System.Security.Principal;

namespace PubliAX.Web.Configs.authentication
{
    public class CustomPrincipal : ICustomPrincipal
    {
        public IIdentity Identity { get; private set; }
        public bool IsInRole(string role) { return role == this.role; }

        public CustomPrincipal(string login, string role)
        {
            this.Identity = new GenericIdentity(login, role);
            this.login = login;
            this.role = role;
        }

        public Guid userId { get; set; }
        public string login { get; set; }
        public string name { get; set; }
        public string celphone { get; set; }
        public string email { get; set; }
        public long codforn { get; set; }

        public string role { get; private set; }
    }

    public class SerializeCustomPrincipal
    {
        public Guid userId { get; set; }
        public string login { get; set; }
        public string name { get; set; }
        public string celphone { get; set; }
        public string email { get; set; }
        public long codforn { get; set; }
        public string role { get; set; }
    }
}