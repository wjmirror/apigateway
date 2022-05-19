//-----------------------------------------------------------------------------
// <copyright file="WebApiTestBaseOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ApiGateway.Test.Utils
{
    /// <summary>
    /// The WebApiTestBase used for the test base class.
    /// </summary>  
    public abstract class WebApiTestBase<TTest, TServerStartUp> : IClassFixture<WebApiTestFixture<TServerStartUp>> 
        where TTest : class
        where TServerStartUp : class
    {
        private WebApiTestFixture<TServerStartUp> _fixture;

        public IConfiguration ServerConfiguration { get; private set; }
 
        /// <summary>
        /// Initializes a new instance of the <see cref="WebODataTestBase{TStartup}"/> class.
        /// </summary>
        /// <param name="fixture">The factory used to initialize the web service client.</param>
        protected WebApiTestBase(WebApiTestFixture<TServerStartUp> fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _fixture.Output = output;
            _fixture.OnServerConfiguation = this.OnServerConfiguation;
            _fixture.OnServerConfigureServices = this.OnServerConfigureServices;
            _fixture.OnServerConfigureTestServices = this.OnServerConfigureTestServices;
        }

        /// <summary>
        /// Set up addtional configuration, it is called before Server Application configuration services
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configBuilder"></param>
        protected virtual void OnServerConfiguation(WebHostBuilderContext context, IConfigurationBuilder configBuilder)
        {
            this.ServerConfiguration = configBuilder
               .AddJsonFile("appsettings.Test.json")
               .Build();
        }

        /// <summary>
        /// Addtional server service setup, it is called after Server Application Configuration services
        /// </summary>
        /// <param name="context"></param>
        /// <param name="services"></param>
        protected virtual void OnServerConfigureServices(WebHostBuilderContext context, IServiceCollection services)
        { 
        }

        /// <summary>
        /// Test services configuration, it is called after OnServerConfigurationServices
        /// </summary>
        /// <param name="services"></param>
        protected virtual void OnServerConfigureTestServices(IServiceCollection services)
        {
            AuthenticationOptions authOption = new AuthenticationOptions();
            this.ServerConfiguration.GetSection(nameof(AuthenticationOptions)).Bind(authOption);
            if (authOption.DefaultScheme == "Test")
            {
                services.AddAuthentication("Test")
                       .AddScheme<TestAuthenticationSchemaOptions, TestAuthenticationHandler>("Test", options =>
                       {
                           this.ServerConfiguration.GetSection(nameof(AuthenticationOptions)).GetSection("Test").Bind(options);
                           this.SetupTestAuthenticationSchemaOptions(options);
                       });
            }
        }

        /// <summary>
        /// Server Application builder pipeline. Please setup fixture.OnServerApplicationBuild=this.OnServerApplicationBuild to replace the server application pipeline.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="app"></param>
        protected virtual void OnServerApplicationBuild(WebHostBuilderContext context, IApplicationBuilder app)
        {

        }

        protected virtual void SetupTestAuthenticationSchemaOptions(TestAuthenticationSchemaOptions options) 
        { }

        /// <summary>
        /// Create the httpClient
        /// </summary>
        /// <returns></returns>
        protected virtual Task<HttpClient> CreateClientAsync()
        {
            var httpClient=_fixture.CreateClient();
            return Task.FromResult(httpClient);
        }
    }
}
