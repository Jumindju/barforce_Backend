using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Barforce_Backend.Helper;
using Barforce_Backend.Helper.Middleware;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Helper.Database;
using Barforce_Backend.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Barforce_Backend.WebSockets;

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

            services.AddSingleton<IDbHelper, DbHelper>();
            services.AddSingleton<IHashHelper, HashHelper>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddWebSocketManager();

            services.AddControllers()
                .AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var wsOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(60),
                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(wsOptions);
            app.MapWebSocketManager("/machine", serviceProvider.GetService<MachineHandler>());

            app.UseHttpStatusCodeExceptionMiddleware();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
