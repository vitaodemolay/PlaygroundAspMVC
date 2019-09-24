using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace PubliAX.Web.Configs.authentication
{
    public interface ICustomPrincipal : IPrincipal
    {
        Guid userId { get; set; }
        string login { get; set; }
        string name { get; set; }
        string celphone { get; set; }
        string email { get; set; }
        Int64 codforn { get; set; }
        string role { get; }
    }
}