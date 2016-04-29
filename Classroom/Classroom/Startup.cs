using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Classroom.Startup))]
namespace Classroom
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
