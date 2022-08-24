using ASP.NET_Core_MVC_Playground.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Microsoft.AspNetCore.Identity.UI.Services;
using ASP.NET_Core_MVC_Playground.Services;
using Westwind.Globalization.AspnetCore;
using Microsoft.AspNetCore.Mvc.Localization;
using Westwind.Globalization;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using ASP.NET_Core_MVC_Playground.Areas.Identity.Data;
using Microsoft.Extensions.Logging;
using ASP.NET_Core_MVC_Playground.Controllers;

namespace ASP.NET_Core_MVC_Playground
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(options =>
            {
                // I prefer Properties over the default `Resources` folder
                // due to namespace issues if you have a Resources type as
                // most people do for shared resources.
                options.ResourcesPath = "Properties";
            });

            // Replace StringLocalizers with Db Resource Implementation
            services.AddSingleton(typeof(IStringLocalizerFactory),
                                  typeof(DbResStringLocalizerFactory));
            services.AddSingleton(typeof(IHtmlLocalizerFactory),
                                  typeof(DbResHtmlLocalizerFactory));


            // Required: Enable Westwind.Globalization (opt parm is optional)
            // shown here with optional manual configuration code
            services.AddWestwindGlobalization(opt =>
            {
                // Make sure the database you connect to exists
                opt.ConnectionString = Configuration["DbResourceConfiguration:ConnectionString"];
                opt.GoogleApiKey = Configuration["DbResourceConfiguration:GoogleApiKey"];
                // DeepL and Microsoft Translator not working in current package implementation
                
                // Set up security for Localization Administration form
                opt.ConfigureAuthorizeLocalizationAdministration(actionContext =>
                {
                    if ((actionContext.HttpContext.User.Identity.IsAuthenticated) && (actionContext.HttpContext.User.IsInRole("Admin"))){
                        return true;
                    }
                    return false;  
                });

            });

            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("en"),
                new CultureInfo("el-GR"),
                new CultureInfo("el"),
            };
            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };
            services.AddSingleton<RequestLocalizationOptions>(localizationOptions);

            // Dbs Context
            services.AddDbContext<Identity.IdentityDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("IdentityDb")));
            services.AddDbContext<DataDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DataDb")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            // Identity
            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<Identity.IdentityDbContext>();

            services.AddControllersWithViews();
            services.AddRazorPages()
                .AddRazorRuntimeCompilation();

            services.AddMvc()
                .AddNewtonsoftJson()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();

            services.AddOptions();
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddTransient<IViewLocalizer, DbResViewLocalizer>();
            services.Configure<StripeOptions>(Configuration.GetSection("Stripe"));
            services.Configure<AuthMessageSenderOptionsSendgrid>(Configuration.GetSection("EmailService:Sendgrid"));
            services.Configure<AuthMessageSenderOptionsTwilio>(Configuration.GetSection("SmsService:Twilio"));

            services.AddScoped(typeof(Helpers));

            services.AddAuthentication().AddGoogle(options => {
                IConfigurationSection googleAuthNSection =
                    Configuration.GetSection("Authentication:Google");

                options.ClientId = googleAuthNSection["ClientId"];
                options.ClientSecret = googleAuthNSection["ClientSecret"];
            });

            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            });

            services.AddAuthentication().AddTwitter(twitterOptions =>
            {
                twitterOptions.ConsumerKey = Configuration["Authentication:Twitter:ApiKey"];
                twitterOptions.ConsumerSecret = Configuration["Authentication:Twitter:SecretKey"];
            });

            services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                microsoftOptions.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
                              IWebHostEnvironment env, 
                              IServiceProvider serviceProvider, 
                              RequestLocalizationOptions options)
        {
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseBrowserLink();
                
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRequestLocalization(options);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            CreateRoles(serviceProvider);
            CreateAdmin(serviceProvider);
        }

        private void CreateRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            
            string[] roles = new string[] { "User", "Admin" };
            foreach (string role in roles)
            {
                Task<bool> roleExists = roleManager.RoleExistsAsync(role);
                roleExists.Wait();

                if (!roleExists.Result)
                {
                    var newRole = new IdentityRole
                    {
                        Name = role
                    };
                    Task<IdentityResult> roleResult = roleManager.CreateAsync(newRole);
                    roleResult.Wait();
                }
            }
        }

        private void CreateAdmin(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var db = serviceProvider.GetRequiredService<DataDbContext>();

            var user = new ApplicationUser
            {
                FirstName = Configuration["AdminAccount:FirstName"],
                LastName = Configuration["AdminAccount:LastName"],
                UserName = Configuration["AdminAccount:Email"],
                Email = Configuration["AdminAccount:Email"]
            };
            Task<IdentityResult> adminUser = userManager.CreateAsync(user, Configuration["AdminAccount:Password"]);
            adminUser.Wait();
            if (adminUser.Result.Succeeded)
            {
                // Assign Roles
                Task<IdentityResult> userRole = userManager.AddToRoleAsync(user, "User");
                userRole.Wait();

                Task<IdentityResult> adminRole = userManager.AddToRoleAsync(user, "Admin");
                adminRole.Wait();

                // Create New Buyer
                Buyer newBuyer = new()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                };
                db.Buyers.Add(newBuyer);
                db.SaveChangesAsync().Wait();

                // Create Associated Shopping Basket
                ShoppingBasket newShoppingBasket = new()
                {
                    BuyerId = newBuyer.Id
                };
                db.ShoppingBaskets.Add(newShoppingBasket);
                db.SaveChangesAsync().Wait();
            }
        }
    }
}
