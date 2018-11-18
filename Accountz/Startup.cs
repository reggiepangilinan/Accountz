using Accountz.Domain;
using Accountz.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Accountz
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

            services.AddDbContext<AccountzDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default")));

            services.AddIdentity<UserAccount, IdentityRole>()
                .AddEntityFrameworkStores<AccountzDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAuthentication(IdentityConstants.ApplicationScheme);

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";
                options.AccessDeniedPath = "/AccessDenied";
            });


            var builder = services.AddIdentityServer(options =>
            {
                options.UserInteraction.LoginUrl = "/Login";
                options.UserInteraction.LogoutUrl = "/Logout";

                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;


                //Cookie Options
                options.Authentication.CookieLifetime = new System.TimeSpan(8, 0, 0); //8hrs
                options.Authentication.CookieSlidingExpiration = true; // Will automatically slide the expiration as long as the app is accessed before reaching the expiration period.
            })
            .AddDeveloperSigningCredential()
            .AddAspNetIdentity<UserAccount>()
            .AddInMemoryIdentityResources(Config.GetIdentityResources())
            .AddInMemoryApiResources(Config.GetApis())
            .AddInMemoryClients(Config.GetClients())
            .AddTestUsers(TestUsers.Users)
            .AddProfileService<ProfileService>();



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            // app.UseAuthentication();
            app.UseIdentityServer();
            app.UseMvc();


        }
    }
}
