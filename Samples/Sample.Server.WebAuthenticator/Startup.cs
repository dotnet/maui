using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Sample.Server.WebAuthenticator
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        IConfiguration Configuration { get; }

        IWebHostEnvironment WebHostEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddAuthentication()
                .AddCookie()
                .AddFacebook(fb =>
                {
                    fb.AppId = Configuration["FacebookAppId"];
                    fb.AppSecret = Configuration["FacebookAppSecret"];
                    fb.SaveTokens = true;
                })
                .AddGoogle(g =>
                {
                    g.ClientId = Configuration["GoogleClientId"];
                    g.ClientSecret = Configuration["GoogleClientSecret"];
                    g.SaveTokens = true;
                })
                .AddMicrosoftAccount(ms =>
                {
                    ms.ClientId = Configuration["MicrosoftClientId"];
                    ms.ClientSecret = Configuration["MicrosoftClientSecret"];
                    ms.SaveTokens = true;
                })
                .AddApple(a =>
                {
                    a.ClientId = Configuration["AppleClientId"];
                    a.KeyId = Configuration["AppleKeyId"];
                    a.TeamId = Configuration["AppleTeamId"];
                    a.UsePrivateKey(keyId
                        => WebHostEnvironment.ContentRootFileProvider.GetFileInfo($"AuthKey_{keyId}.p8"));
                    a.SaveTokens = true;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
