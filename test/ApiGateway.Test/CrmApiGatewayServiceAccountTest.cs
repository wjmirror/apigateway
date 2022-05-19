using ApiGateway.CRM;
using ApiGateway.Test.Utils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ApiGateway.Test
{
    //[Collection("Sequential")]
    public class CrmApiGatewayServiceAccountTest : CrmApiGatewayImpersonateAsLoginUserTest
    {
        private readonly ITestOutputHelper _output;
        private readonly WebApiTestFixture<Startup> _fixture;

        public CrmApiGatewayServiceAccountTest(WebApiTestFixture<Startup> fixture, ITestOutputHelper output) : base(fixture, output)
        {
            _output = output;
            _fixture = fixture;
        }
        protected override string GetContactEmail()
        {
            return "jim.wang1019_serviceAccount@spray.com";
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