using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SessionStateDemo.Startup))]
namespace SessionStateDemo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
