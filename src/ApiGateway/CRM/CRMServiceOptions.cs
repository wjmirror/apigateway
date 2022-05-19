using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiGateway.CRM
{
    public partial class CRMServiceOptions
    {
        public CRMServiceOptions()
        {
            this.AuthenticationOptions = new NTLMAuthenticationOption();
        }
        public string ServiceUrl { get; set; }  
        public string ApiVersion { get; set; } = "v9.2";
        public Guid CallerObjectId { get; set; }

        public string ServiceAccount { get; set; }
        public AuthenticationOption AuthenticationOptions { get; set; } 


        public static CRMServiceOptions ReadFromJsonConfig(IConfiguration configuration)
        {
            CRMServiceOptions serviceOptions;
            IConfigurationSection configSection = configuration.GetSection("CRMService");
            string serviceUrl = configSection.GetValue<string>("ServiceUrl");
            string apiVersion = configSection.GetValue<string>("ApiVersion");
            Guid callerIdGuid = Guid.Empty;
            string callerId= configSection.GetValue<string>("CallerObjectId");
            if (!string.IsNullOrEmpty(callerId))
                callerIdGuid = Guid.Parse(callerId);

            string serviceAccount = configSection.GetValue<string>("ServiceAccount");

            IConfigurationSection authSection = configSection.GetSection("AuthenticationOptions");
            AuthenticationMethodEnum authenticationMethod = (AuthenticationMethodEnum)Enum.Parse(typeof(AuthenticationMethodEnum), authSection.GetValue<string>("AuthenticationMethod"), true);
            switch (authenticationMethod)
            {
                case AuthenticationMethodEnum.NTLM:
                    serviceOptions = new CRMServiceOptions
                    {
                        ServiceUrl = serviceUrl,
                        ApiVersion = apiVersion,
                        CallerObjectId = callerIdGuid,
                        ServiceAccount = serviceAccount,
                        AuthenticationOptions = authSection.Get<NTLMAuthenticationOption>()
                    };
                    break;
                case AuthenticationMethodEnum.OAuth2:
                    serviceOptions = new CRMServiceOptions
                    {
                        ServiceUrl = serviceUrl,
                        ApiVersion = apiVersion,
                        CallerObjectId = callerIdGuid,
                        ServiceAccount = serviceAccount,
                        AuthenticationOptions = authSection.Get<OAuth2AuthenticationOption>()
                    };
                    break;
                default:
                    serviceOptions = new CRMServiceOptions
                    {
                        ServiceUrl = serviceUrl,
                        ApiVersion = apiVersion,
                        CallerObjectId = callerIdGuid,
                        ServiceAccount = serviceAccount,
                        AuthenticationOptions = new AuthenticationOption()
                    };
                    break;
            }

            return serviceOptions;
        }
        public static CRMServiceOptions ReadFromXmlConfig(IConfiguration configuration)
        {
            throw new NotImplementedException("Please Create your CRMServiceOpitons from Reading Web.config/app.config.");
        }
    }

    public partial class AuthenticationOption
    {
        public virtual AuthenticationMethodEnum AuthenticationMethod { get; set; } = AuthenticationMethodEnum.None;
    }

    public partial class NTLMAuthenticationOption:AuthenticationOption
    {
        public NTLMAuthenticationOption()
        {
            base.AuthenticationMethod = AuthenticationMethodEnum.NTLM;
        }
        public bool UseDefaultNetworkCredential { get; set; } = false;
        public string UserDomain { get; set; }
        public string UserName { get; set; } 
        public string Password { get; set; } 
    }

    public partial class OAuth2AuthenticationOption: AuthenticationOption
    {
        public OAuth2AuthenticationOption()
        {
            base.AuthenticationMethod = AuthenticationMethodEnum.OAuth2;
        }

        public string Authority { get; set; } 
        public  string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ResourceId { get; set; }
        public string UserPrincipalName { get; set; } 
        public string Password { get; set; }
        public bool UseUserAssertion { get; set; }
    }

    public enum AuthenticationMethodEnum
    {
        None = 0,
        NTLM = 1,
        OAuth2 = 2
    }
}
