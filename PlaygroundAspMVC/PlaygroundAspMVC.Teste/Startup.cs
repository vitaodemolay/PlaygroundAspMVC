using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PlaygroundAspMVC.Teste.Startup))]
namespace PlaygroundAspMVC.Teste
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
