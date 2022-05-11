using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;

namespace ApiGateway.CRM
{
    public class CRMProxyConfigFilter :IProxyConfigFilter
    {
        private readonly CRMServiceOptions _crmServiceOptions;
        public CRMProxyConfigFilter(IOptions<CRMServiceOptions> options)
        {
            _crmServiceOptions = options.Value;
        }

        public ValueTask<ClusterConfig> ConfigureClusterAsync(ClusterConfig cluster, CancellationToken cancel)
        {
            if (string.Compare(cluster.ClusterId, "crmCluster", true) != 0)
                return new ValueTask<ClusterConfig>(cluster);

            var newDesintations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase);
            foreach (var d in cluster.Destinations)
            {
                var originalAddress = d.Value.Address;
                var newAddress=originalAddress.Replace("{CRMService.ServiceUrl}", _crmServiceOptions.ServiceUrl);
                var modifiedDest= d.Value with {  Address= newAddress };
                newDesintations.Add(d.Key, modifiedDest);
            }
            return new ValueTask<ClusterConfig>(cluster with { Destinations = newDesintations });
        }

        public ValueTask<RouteConfig> ConfigureRouteAsync(RouteConfig route, ClusterConfig cluster, CancellationToken cancel)
        {
            if (string.Compare(route.RouteId, "crmRoute", true) != 0)
                return new ValueTask<RouteConfig>(route);

            var newTransformList= new List<Dictionary<string, string>>();
            foreach(var trans in route.Transforms)
            {
                var newDict=new Dictionary<string, string>();
                foreach (var entry in trans)
                {
                    if(entry.Key == "PathPattern")
                    {
                        var newPatter = entry.Value.Replace("{CRMService.ApiVersion}", _crmServiceOptions.ApiVersion);
                        newDict.Add(entry.Key, newPatter);
                    }
                    else
                    {
                        newDict.Add(entry.Key,entry.Value);
                    }
                }
                newTransformList.Add(newDict);
            }
            return new ValueTask<RouteConfig>(route with { Transforms = newTransformList });
        }
    }
}
