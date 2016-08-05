using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlComparer.Implementation;
using SqlComparer.Parsing;
using SqlComparer.Web.Models.Options;
using SqlComparer.Web.Services;
using SqlComparer.Web.Services.Implementation;
using NLog.Extensions.Logging;
using SqlComparer.Web.Services.Authorization;
using SqlComparer.Web.Services.Authorization.HasMinimumPermission;

namespace SqlComparer.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddDistributedMemoryCache();
            services.AddSession();

            // Configuration
            services.AddOptions();
            services.Configure<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<DatabaseSettings>(Configuration.GetSection("DatabaseSettings"));
            services.Configure<Permissions>(Configuration.GetSection("Permissions"));
            services.Configure<ComparisonSettings>(Configuration.GetSection("ComparisonSettings"));
            
            // Add dependencies
            services.AddTransient<IComparer, Comparer>();
            services.AddTransient<IComparedEntityFactory, ComparedEntityFactory>();
            services.AddTransient<ITSqlFragmentFactory, TSqlFragmentFactory>();
            services.AddTransient<ISqlComparerService, SqlComparerService>();
            services.AddTransient<IIdentifierService, IdentifierService>();
            services.AddTransient<IOptionsService, OptionsService>();
            services.AddTransient<IConnectionService, ConnectionService>();
            services.AddTransient<IDatabaseEntityRepository, DatabaseEntityRepository>();
            
            // Authorization
            services.AddAuthorization(options =>
            {
                var optionsService = services.BuildServiceProvider().GetService<IOptionsService>();
                options.AddPolicy("MinimumAccessPermission", policy => policy.Requirements.Add(new HasMinimumPermissionRequirement(optionsService)));
            });
            services.AddSingleton<IAuthorizationHandler, HasMinimumPermissionHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddNLog();
            
            env.ConfigureNLog("nlog.config");
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            
            app.UseStaticFiles();

            app.UseSession();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Search}/{action=Index}/{id?}");
            });
        }
    }
}
