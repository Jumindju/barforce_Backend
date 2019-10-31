using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Barforce_Backend.Helper;
using Barforce_Backend.Helper.Middleware;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Configuration;
using Barforce_Backend.Model.Helper.Database;
using Barforce_Backend.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Barforce_Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<DbSettings>(Configuration.GetSection("DbSettings"));
            services.Configure<JwtOptions>(Configuration.GetSection("JwtOptions"));

            services.AddSingleton<IDbHelper, DbHelper>();
            services.AddSingleton<IHashHelper, HashHelper>();
            services.AddSingleton<ITokenHelper, TokenHelper>();

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddControllers()
                .AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpStatusCodeExceptionMiddleware();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
