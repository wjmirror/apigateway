//-----------------------------------------------------------------------------
// <copyright file="WebApiTestFixtureOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ApiGateway.Test.Utils
{
    public class WebApiTestFixture<TServerStartUp> : IDisposable 
        where TServerStartUp : class
    {

        public ITestOutputHelper Output { get; set; }

        /// <summary>
        /// The test server.
        /// </summary>
        private WebApplicationFactory<TServerStartUp> _factory = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiTestFixture{T}"/> class
        /// </summary>
        public WebApiTestFixture()
        {
        }

        /// <summary>
        /// Create the <see cref="HttpClient"/>.
        /// </summary>
        /// <returns>The created HttpClient.</returns>
        public HttpClient CreateClient()
        {
            WebApplicationFactoryClientOptions options = new WebApplicationFactoryClientOptions();
            options.HandleCookies = false;
            var client = this.ApplicationFactory.CreateClient(options);
            client.DefaultRequestHeaders.Add("OData-Version", "4.0");
            client.DefaultRequestHeaders.Add("prefer", "return=representation");

            return client;
        }

        public WebApplicationFactory<TServerStartUp> ApplicationFactory
        {
            get
            {
                if (_factory == null)
                    this.InitializeAppFactory();

                return _factory;
            }
        }

        /// <summary>
        /// Cleanup the server.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        /// <summary>
        /// Cleanup the server.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_factory != null)
                {
                    _factory.Dispose();
                    _factory = null;
                }
            }
        }

        public Action<WebHostBuilderContext,IConfigurationBuilder> OnServerConfiguation { get; set; }
        public Action<WebHostBuilderContext,IServiceCollection> OnServerConfigureServices { get; set; }

        public Action<IServiceCollection> OnServerConfigureTestServices { get; set; }
        public Action<WebHostBuilderContext,IApplicationBuilder> OnServerApplicationBuild { get; set; }

        /// <summary>
        /// Initialize the fixture.
        /// </summary>
        private void InitializeAppFactory()
        {
            _factory = new WebApplicationFactory<TServerStartUp>()
                .WithWebHostBuilder(builder =>
                {
                    if (this.OnServerConfiguation != null)
                        builder.ConfigureAppConfiguration((context, configBuilder) =>
                        {
                            this.OnServerConfiguation(context, configBuilder);
                        });

                    if (this.OnServerConfigureServices != null)
                        builder.ConfigureServices((context, services) =>
                        {
                            this.OnServerConfigureServices(context, services);
                        });

                    if (this.OnServerConfigureTestServices != null)
                        builder.ConfigureTestServices(services =>
                        {
                            this.OnServerConfigureTestServices(services);
                        });

                    if (this.OnServerApplicationBuild != null)
                        builder.Configure((context, app) =>
                        {
                            this.OnServerApplicationBuild(context, app);
                        });
                });
            Output?.WriteLine($"Test Server {typeof(TServerStartUp).FullName} Initialized.");
        }
    }
}
