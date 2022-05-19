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
    public class CrmApiGatewayImpersonateAsLoginUserTest : ApiGatewayTestBase<CrmApiGatewayImpersonateAsLoginUserTest>
    {
        private readonly ITestOutputHelper _output;
        private readonly WebApiTestFixture<Startup> _fixture;

        private string _userNetworkId = "[domain]\\wangji";

        public CrmApiGatewayImpersonateAsLoginUserTest(WebApiTestFixture<Startup> fixture, ITestOutputHelper output) : base(fixture, output)
        {
            _output = output;
            _fixture = fixture;
        }

        protected override void SetupTestAuthenticationSchemaOptions(TestAuthenticationSchemaOptions options)
        {
            base.SetupTestAuthenticationSchemaOptions(options);
            options.IdentityName = _userNetworkId;
        }

        protected virtual string GetContactEmail()
        {
            return "jim.wang1019@spray.com.cn";
        }

        [Fact]
        public async Task Test_Connect_CrmApi_Successful()
        {
            var client = await this.CreateClientAsync();
            string url = "/crmapi/accounts?$top=1";
            var response = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Get:{url}");
            _output.WriteLine(content);
            Assert.False(string.IsNullOrWhiteSpace(content));
        }
        [Fact]
        public async Task Test_Create_AccountAsync()
        {
            var client = await this.CreateClientAsync();
            var surl = $"/crmapi/contacts?$select=contactid&$filter=emailaddress1 eq '{this.GetContactEmail()}'";
            _output.WriteLine($"Get: {surl}");
            var searchRes = await client.GetAsync(surl);
            _output.WriteLine($"\tStatus: {searchRes.StatusCode}");

            var searchContent= await searchRes.Content.ReadAsStringAsync();
            _output.WriteLine($"\t{searchContent}");
            Dictionary<string, JsonElement> dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(searchContent);
            
            JsonElement jsonElement;
            if (dict != null && dict.TryGetValue("value", out jsonElement))
            {
                if(jsonElement.ValueKind == JsonValueKind.Array)
                {
                    foreach(var json in jsonElement.EnumerateArray())
                    {
                        Dictionary<string, string> con = json.Deserialize<Dictionary<string, string>>();
                        string contactId;
                        if(con.TryGetValue("contactid", out contactId))
                        {
                            var deleteRes = await client.DeleteAsync($"/crmapi/contacts({contactId})");
                            Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

                        }
                    }
                }
            }

            string contact = $$"""
{
    "firstname": "Jim",
    "lastname": "Wang",
    "middlename": "0518",
    "emailaddress1": "{{this.GetContactEmail()}}",
    "mobilephone": "6306497777"
}
""";
            var contactContent = new StringContent(contact);
            contactContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync("/crmapi/contacts", contactContent);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Contact Created: {content}");

            var contactDict= System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
            Assert.True(contactDict.ContainsKey("contactid"));
            

        }
    }
}