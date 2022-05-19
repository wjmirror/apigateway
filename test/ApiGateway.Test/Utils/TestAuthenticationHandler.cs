using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ApiGateway.Test.Utils
{
    public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationSchemaOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<TestAuthenticationSchemaOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] {
                new Claim(ClaimTypes.Name, this.Options.IdentityName),
                new Claim(ClaimTypes.Role, this.Options.Role)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }

    public class TestAuthenticationSchemaOptions : AuthenticationSchemeOptions
    {
        public TestAuthenticationSchemaOptions() : base()
        {
        }

        public string IdentityName { get; set; }
        public string Role { get; set; }
    }

    public class TestOAuthOptions
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ResourceId { get; set; }
        public string[] Scopes { get; set; }
    }
}
