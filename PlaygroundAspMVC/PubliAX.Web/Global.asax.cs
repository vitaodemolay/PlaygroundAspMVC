using PubliAX.Web.Configs.authentication;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace PubliAX.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }


        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                JavaScriptSerializer serializer = new JavaScriptSerializer();

                SerializeCustomPrincipal serializeModel = serializer.Deserialize<SerializeCustomPrincipal>(authTicket.UserData);

                CustomPrincipal newUser = new CustomPrincipal(authTicket.Name, serializeModel.role);
                newUser.userId = serializeModel.userId;
                newUser.name = serializeModel.name;
                newUser.email = serializeModel.email;
                newUser.celphone = serializeModel.celphone;
                newUser.codforn = serializeModel.codforn;


                HttpContext.Current.User = newUser;
            }
        }
    }
}
