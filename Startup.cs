using Hangfire;
using Hangfire.Dashboard;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Trinbago_MVC5.Startup))]
namespace Trinbago_MVC5
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // Hangfire configure
            GlobalConfiguration.Configuration.UseSqlServerStorage("ConnectionB");
            var options = new DashboardOptions
            {
                AuthorizationFilters = new[]
            {
                new AuthorizationFilter { Roles = "Admin" }
            }
            };
            app.UseHangfireDashboard("/hangfire", options);
            app.UseHangfireServer();
        }
    }

}
