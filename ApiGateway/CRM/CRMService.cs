using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.Identity.Client;
using System.Security;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;

namespace ApiGateway.CRM
{
    public class CRMService
    {
        private readonly CRMServiceOptions _serviceOptions;
        private readonly string _apiUrl;

        public CRMService(CRMServiceOptions options)
        {
            this._serviceOptions = options;
            this._apiUrl = options.ServiceUrl.Trim() + (options.ServiceUrl.Trim().EndsWith("/") ? "" : "/") + $"api/data/{this._serviceOptions.ApiVersion}/";
        }

        public async Task<string> GetOAuthToken()
        {
            OAuth2AuthenticationOption option = (OAuth2AuthenticationOption)this._serviceOptions.AuthenticationOptions;

            var clientApp = PublicClientApplicationBuilder
                .Create(option.ClientId)
                .WithAuthority(option.Authority)
                .Build();
            clientApp.UserTokenCache.SetCacheOptions(new CacheOptions(true));

            var accounts = await clientApp.GetAccountsAsync();

            var scope = this._serviceOptions.ServiceUrl.Trim();
            if (!scope.EndsWith("/"))
                scope = scope + "/";

            scope = scope + ".default";
            var scopes = new string[] { scope };

            AuthenticationResult authResult = null;

            var crmAccount = accounts.FirstOrDefault(acc => string.Compare(acc.Username, option.UserPrincipalName, true) == 0);


            if (crmAccount != null)
            {
                authResult = await clientApp.AcquireTokenSilent(scopes, crmAccount)
                                  .ExecuteAsync();
            }
            else
            {
                var secPassword = new SecureString();
                foreach (char c in option.Password)
                    secPassword.AppendChar(c);

                try
                {
                    authResult = await clientApp.AcquireTokenByUsernamePassword(scopes, option.UserPrincipalName, secPassword).ExecuteAsync();
                }
                catch (MsalUiRequiredException ex)
                {
                    //See https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Username-Password-Authentication
                    string errorMessage = string.Empty;
                    if (ex.Message.Contains("AADSTS65001"))
                    {
                        errorMessage = $"The user {option.UserPrincipalName} or administrator has not consented to use the application, application Id {option.ClientId}. ";
                    }

                    if (ex.Message.Contains("AADSTS50079"))
                    {
                        errorMessage = $"The user {option.UserPrincipalName} is reqired to use multi-factor authentication, can not use use username/password to authenticate.";

                    }

                    if (ex.Message.Contains("AADSTS70002"))
                    {
                        errorMessage = $"Invalid user name {option.UserPrincipalName} or password.";

                    }

                    if (ex.Message.Contains("ADSTS50034"))
                    {
                        errorMessage = $"The user {option.UserPrincipalName} must be added to active directory.";

                    }

                    if (string.IsNullOrWhiteSpace(errorMessage))
                        errorMessage = ex.Message;
                    else
                        errorMessage = errorMessage + "\r\n" + ex.Message;

                    throw new Exception(errorMessage, ex);

                }
                catch (MsalServiceException ex)
                {
                    string errorMessage = string.Empty;
                    if (ex.ErrorCode == "invalid_request")
                    {
                        errorMessage = "The grant type is not supported over the /common or /consumers endpoints";
                    }
                    if (ex.ErrorCode == "unauthorized_client")
                    {
                        errorMessage = $"Application with identifier '{option.ClientId}' was not found in the directory";
                    }
                    if (ex.ErrorCode == "invalid_client")
                    {
                        errorMessage = $"The request body must contain the following parameter: 'client_secret or client_assertion'";
                    }

                    if (string.IsNullOrWhiteSpace(errorMessage))
                        errorMessage = ex.Message;
                    else
                        errorMessage = errorMessage + "\r\n" + ex.Message;

                    throw new Exception(errorMessage, ex);

                }
                catch (MsalClientException ex)
                {
                    string errorMessage = string.Empty;
                    if (ex.ErrorCode == "unknown_user_type" ||
                        ex.ErrorCode == "user_realm_discovery_failed" ||
                        ex.ErrorCode == "unknown_user" ||
                        ex.ErrorCode == "parsing_wstrust_response_failed")
                    {
                        errorMessage = $"Unsupported User Type, unknow user, wrong user.";
                    }

                    if (string.IsNullOrWhiteSpace(errorMessage))
                        errorMessage = ex.Message;
                    else
                        errorMessage = errorMessage + "\r\n" + ex.Message;

                    throw new Exception(errorMessage, ex);
                }
            }
            return authResult?.AccessToken;
        }

        public async Task<string> GetUserObjectId(IIdentity identity)
        {
            string objectId = null;
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(this._apiUrl);

            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Add("prefer", "return=representation");

            string authToken = await this.GetOAuthToken();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");

            string query = $"systemusers?$filter=ssco_networkid eq '{identity.Name}' and deletedstate eq 0 &$select=ssco_networkid,azureactivedirectoryobjectid,systemuserid";

            var response = await httpClient.GetStringAsync(query);

            Dictionary<string, JsonElement> ret = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(response);
            if (ret.ContainsKey("value"))
            {
                JsonElement jelement = ret["value"];
                var users = jelement.Deserialize<List<systemuser>>();
                if (users != null && users.Count == 1)
                {
                    objectId = users[0].azureactivedirectoryobjectid;
                }
            }

            
            return objectId;
        }

        public class systemuser
        {
            public string ssco_networkid { get; set; }
            public string azureactivedirectoryobjectid { get; set; }
            public string systemuserid { get; set; }
            public string ownerid { get; set; }

        }
    }
}
