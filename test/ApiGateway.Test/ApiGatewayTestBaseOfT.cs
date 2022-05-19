using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiGateway.Test.Utils;
using Xunit.Abstractions;

namespace ApiGateway.Test
{
    public class ApiGatewayTestBase<TTest> : WebApiTestBase<TTest, ApiGateway.Startup>
        where TTest : class
    {
        public ApiGatewayTestBase(WebApiTestFixture<Startup> fixture, ITestOutputHelper output) : base(fixture,output)
        {
        }

    }
}
