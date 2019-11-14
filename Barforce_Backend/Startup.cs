using System.Text;
using Barforce_Backend.Helper;
using Barforce_Backend.Helper.Middleware;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Configuration;
using Barforce_Backend.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Barforce_Backend.WebSockets;
using Microsoft.IdentityModel.Tokens;
using System;

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
            services.Configure<EMailOptions>(Configuration.GetSection("EmailOptions"));

            services.AddSingleton<IDbHelper, DbHelper>();
            services.AddSingleton<IHashHelper, HashHelper>();
            services.AddSingleton<ITokenHelper, TokenHelper>();
            services.AddSingleton<IEmailHelper, EMailHelper>();

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddWebSocketManager();

            var jwtOptions = Configuration.GetSection("JwtOptions").Get<JwtOptions>();
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "barforce_tm",
                        IssuerSigningKey = symmetricSecurityKey
                    };
                });

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

            var wsOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(60),
                ReceiveBufferSize = 4 * 1024
            };
            var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            var serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;

            app.UseWebSockets(wsOptions);
            app.MapWebSocketManager("/machine", serviceProvider.GetService<MachineHandler>());
            app.UseStaticFiles();

            app.UseAuthorization();

            app.UseHttpStatusCodeExceptionMiddleware();
            app.UseTokenValidateMiddleware();

            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}