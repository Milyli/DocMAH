using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DocMAH.MvcHarness.Startup))]
namespace DocMAH.MvcHarness
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
