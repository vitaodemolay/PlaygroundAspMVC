using System.Web.Http;
using System.Web.Security;

namespace PlaygroundAspMVC.MvcAuthTeste.Controllers.Apis
{
    public class AuthController : ApiController
    {
        
        [HttpPost]
        [Route("api/auth/logoff")]
        public void logoff()
        {
            FormsAuthentication.SignOut();
        }
    }
}
