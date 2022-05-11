using Microsoft.Extensions.Options;
using System;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace ApiGateway.CRM
{
    public class CRMProxyTransformProvider:ITransformProvider
    {
        private readonly CRMServiceOptions _crmServiceOptions;
        public CRMProxyTransformProvider(IOptions<CRMServiceOptions> options)
        { 
            _crmServiceOptions = options.Value;
        }

        public void Apply(TransformBuilderContext context)
        {
            if (string.Compare(context.Route.RouteId, "crmRoute", true) != 0)
                return;

            context.AddRequestTransform(async transformContext =>
            {
                CRMService service = new CRMService(_crmServiceOptions);
                string authToken = await service.GetOAuthToken();

                transformContext.ProxyRequest.Headers.Remove("Authorization");
                transformContext.ProxyRequest.Headers.Add("Authorization", $"Bearer {authToken}");

                //check mscrm caller Id 
                var user = transformContext.HttpContext.User.Identity;
                
                if(string.Compare(user.Name, _crmServiceOptions.ServiceAccount,true) != 0) //inbound user is not a service account
                {
                    transformContext.ProxyRequest.Headers.Remove("CallerObjectId");
                    transformContext.ProxyRequest.Headers.Remove("MSCRMCallerID");
                    string userObjectId = null;
                    try
                    {
                        userObjectId = await service.GetUserObjectId(user);
                        transformContext.ProxyRequest.Headers.Add("CallerObjectId", userObjectId);
                    }
                    catch(Exception ex)
                    {
                        throw new Exception($"Can not find Network user {user.Name} in MSCRM.",ex);
                    }
                }
                else //inbound user is a service account user
                {
                    if(!transformContext.ProxyRequest.Headers.Contains("CallerObjectId") && 
                        !transformContext.ProxyRequest.Headers.Contains("MSCRMCallerID") &&
                        _crmServiceOptions.CallerObjectId != Guid.Empty)
                    {
                        transformContext.ProxyRequest.Headers.Add("CallerObjectId", _crmServiceOptions.CallerObjectId.ToString());
                    }
                }
            });
        }

        public void ValidateCluster(TransformClusterValidationContext context)
        {
            return;
        }

        public void ValidateRoute(TransformRouteValidationContext context)
        {
            return;
        }
    }
}
