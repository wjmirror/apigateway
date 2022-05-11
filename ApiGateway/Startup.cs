using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Transforms;
using ApiGateway.CRM;

namespace ApiGateway
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            CRMServiceOptions crmServiceOptions=CRMServiceOptions.ReadFromJsonConfig(_configuration);
            services.AddSingleton<IOptions<CRMServiceOptions>>(Options.Create<CRMServiceOptions>(crmServiceOptions));
 
            services.AddReverseProxy()
                .LoadFromConfig(_configuration.GetSection("ReverseProxy"))
                .AddConfigFilter<CRMProxyConfigFilter>()
                .AddTransforms<CRMProxyTransformProvider>();
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
                endpoints.MapReverseProxy();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("The Api Gateway is running, backed by Yarp!");
                });
            });
        }
    }
}
