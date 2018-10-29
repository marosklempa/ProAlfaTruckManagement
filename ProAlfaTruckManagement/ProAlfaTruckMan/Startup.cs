using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;
using ProAlfaTruckMan.Models;
using ProAlfaTruckMan.Pages;

namespace ProAlfaTruckMan
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Enviroment { get; set; }

        public Startup(IConfiguration configuration, IHostingEnvironment enviroment)
        {
            Configuration = configuration;
            Enviroment = enviroment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});

            //Add localization support
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // IdentityServer4 configuration
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryApiResources(IdentityServer4Config.GetApiResources())
                .AddInMemoryClients(IdentityServer4Config.GetClients());

            services.AddSingleton<IValidationAttributeAdapterProvider, CustomValidationAttributeAdapterProvider>();

            // Add patched IViewLocalizer
            services.AddTransient<IViewLocalizer, ViewLocalizer>();

            //services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();
            services.AddSession(opts =>
            {
                opts.Cookie.Name = "." + Includes.AppShortName;
                opts.IdleTimeout = TimeSpan.FromMinutes(15);
            });  // Session

            // Email service
            services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
            services.AddTransient<IEmailService, EmailService>();

            // OAuth2 authentication for API
            services.AddMvcCore()
                .AddAuthorization()
                .AddJsonFormatters();
            services.AddAuthentication("Bearer").AddIdentityServerAuthentication(options =>
            {
                options.Authority = Enviroment.IsDevelopment() ? "https://localhost:44368/" : "https://proalfa.protherm.sk/";
                options.RequireHttpsMetadata = false;
                options.ApiName = "proalfatruckman";
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseBrowserLink();
                ConnString.Value = "Server=localhost\\SQLEXPRESS;Database=---;User Id=---;Password=---";
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                ConnString.Value = "Server=s1.aspify.com;Database=---;User Id=---;Password=---";
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            // IMPORTANT: This session call MUST go before UseMvc()
            app.UseSession();

            var supportedCultures = new[]{
                new CultureInfo("en-GB"),
                new CultureInfo("sk-SK"),
                new CultureInfo("cs-CZ"),
                new CultureInfo("pl-PL"),
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-GB"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            // IdentityServer4 initialization
            app.UseIdentityServer();

            // OAuth2 authentication for RESTFul API
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }
    }

    public class ProAlfaTruckManagementDbContext : DbContext
    {
        public ProAlfaTruckManagementDbContext(DbContextOptions<ProAlfaTruckManagementDbContext> options) : base(options)
        { }

        public DbSet<Sapvendor_pers> Sapvendors_pers { get; set; }

    }

    public class CustomValidationAttributeAdapterProvider : ValidationAttributeAdapterProvider, IValidationAttributeAdapterProvider
    {
        public CustomValidationAttributeAdapterProvider()
        {
        }

        IAttributeAdapter IValidationAttributeAdapterProvider.GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer stringLocalizer)
        {
            var adapter = base.GetAttributeAdapter(attribute, stringLocalizer);
            if (attribute is CheckUserPwd)
            {
                var minLengthSix = attribute as CheckUserPwd;
                adapter = new UserNameExistAdapter(minLengthSix, stringLocalizer);
            }
            else
            {
                if (attribute is CheckCalWorkDay)
                {
                    var minLengthSix2 = attribute as CheckCalWorkDay;
                    adapter = new CalWorkDayAdapter(minLengthSix2, stringLocalizer);
                }
                else
                {
                    if (attribute is PwdNotEqual)
                    {
                        var minLengthSix3 = attribute as PwdNotEqual;
                        adapter = new PwdNotEqualAdapter(minLengthSix3, stringLocalizer);
                    }
                    else
                    {
                        if (attribute is PwdSameAsOld)
                        {
                            var minLengthSix4 = attribute as PwdSameAsOld;
                            adapter = new PwdSameAsOldAdapter(minLengthSix4, stringLocalizer);
                        }
                    }
                }
            }

            return adapter;
        }
    }

}
