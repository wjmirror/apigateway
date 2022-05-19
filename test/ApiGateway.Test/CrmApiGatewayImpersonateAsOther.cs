using ApiGateway.CRM;
using ApiGateway.Test.Utils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ApiGateway.Test
{
    //[Collection("Sequential")]
    public class CrmApiGatewayImpersonateAsOther : CrmApiGatewayImpersonateAsLoginUserTest
    {
        private readonly ITestOutputHelper _output;
        private readonly WebApiTestFixture<Startup> _fixture;

        public CrmApiGatewayImpersonateAsOther(WebApiTestFixture<Startup> fixture, ITestOutputHelper output) : base(fixture, output)
        {
            _output = output;
            _fixture = fixture;
        }
        protected override async Task<HttpClient> CreateClientAsync()
        {
            var httpClient= await base.CreateClientAsync();

            string otherUser = "[domain]\\wangji";
            CRMServiceOptions crmOptions = (_fixture.ApplicationFactory.Services.GetService(typeof(IOptions<CRMServiceOptions>)) as IOptions<CRMServiceOptions>)?.Value;

            CRMService service=new CRMService(crmOptions);

            string userObjectId = await service.GetUserObjectIdAsync(new GenericIdentity(otherUser));

            httpClient.DefaultRequestHeaders.Add("CallerObjectId", userObjectId);

            return httpClient;
        }
        protected override string GetContactEmail()
        {
            return "jim.wang1019Impersonate@spray.com";
        }
        protected override void SetupTestAuthenticationSchemaOptions(TestAuthenticationSchemaOptions options)
        {
            base.SetupTestAuthenticationSchemaOptions(options);

            //setup run as service account
            CRMServiceOptions crmOptions = (_fixture.ApplicationFactory.Services.GetService(typeof(IOptions<CRMServiceOptions>)) as IOptions<CRMServiceOptions>)?.Value;
            options.IdentityName = crmOptions.ServiceAccount;
        }

    }
}