using ElCamino.AspNetCore.Identity.AzureTable.Model;
using FastiCalSync.Data;
using FastiCalSync.UI.Data;
using FastiCalSync.UI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FastiCalSync.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddAzureTableStores<ApplicationDbContext>(() => new IdentityConfiguration
                {
                    StorageConnectionString = Configuration.GetSection("ConnectionStrings:AzureStorage").Value
                });

            // Add application services.
            services.AddScoped(p => new CalendarRepository(Configuration.GetSection("ConnectionStrings:AzureStorage").Value));

            services.AddMvc(o =>
            {
                o.Filters.Add(new RequireHttpsAttribute());
            });

            services
                .AddAuthentication()
                .AddGoogle(o =>
                {
                    o.ClientId = Configuration["GoogleClientID"];
                    o.ClientSecret = Configuration["GoogleClientSecret"];
                    o.Scope.Add("https://www.googleapis.com/auth/calendar");
                    o.AccessType = "offline";
                    o.SaveTokens = true;
                });

            services.ConfigureApplicationCookie(o =>
            {
                o.LoginPath = "/sign-in";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
