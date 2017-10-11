using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(dnorwoodBugTracker.Startup))]
namespace dnorwoodBugTracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
