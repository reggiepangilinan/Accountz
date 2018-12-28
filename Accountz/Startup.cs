using Accountz.Domain;
using Accountz.Persistence;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

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
            var connectionString = Configuration.GetConnectionString("Default");

            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<AccountzDbContext>(options => options.UseSqlServer(connectionString));

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
                options.UserInteraction.ErrorUrl = "/Error";

                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;


                //Cookie Options
                options.Authentication.CookieLifetime = new System.TimeSpan(8, 0, 0); //8hrs
                options.Authentication.CookieSlidingExpiration = true; // Will automatically slide the expiration as long as the app is accessed before reaching the expiration period.
            })
            .AddDeveloperSigningCredential(false)
            .AddAspNetIdentity<UserAccount>()
            //.AddInMemoryIdentityResources(Config.GetIdentityResources())
            //.AddInMemoryApiResources(Config.GetApis())
            //.AddInMemoryClients(Config.GetClients())
            .AddConfigurationStore(options =>
            {
                options.DefaultSchema = "idsrv4config";
                options.ConfigureDbContext = b => {
                    b.UseSqlServer(connectionString,
                        sql => {
                            sql.MigrationsAssembly(migrationAssembly);
                        });
                };
                    
            })
            .AddOperationalStore(options =>
            {
                options.DefaultSchema = "idsrv4op";
                options.ConfigureDbContext = b =>
                    b.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationAssembly));

                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;
                //options.TokenCleanupInterval = 30;
            })
            .AddTestUsers(TestUsers.Users)
            .AddProfileService<ProfileService>();        
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, AccountzDbContext accountzDbContext)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Error");


            }

            app.UseHsts();
            app.UseHttpsRedirection();


            InitializeIdentityServer4Database(app);
            app.UseStaticFiles();
            app.UseCookiePolicy();
            // app.UseAuthentication(); // Already handled by UseIdentityServer()
            app.UseIdentityServer();
            app.UseMvc();

        }

        private void InitializeIdentityServer4Database(IApplicationBuilder app)
        {
            //http://docs.identityserver.io/en/latest/quickstarts/8_entity_framework.html

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetApis())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
