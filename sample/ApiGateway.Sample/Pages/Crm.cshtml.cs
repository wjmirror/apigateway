using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace ApiGateway.Sample.Pages
{
    public class CrmModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string AccountNumber { get; set; }

        [BindProperty]
        public string Result { get; set; }
        public async Task OnGet()
        {
            if (!string.IsNullOrWhiteSpace(AccountNumber))
            {
                var uri = new Uri("https://sscoweb12dev.spray.com/");
                var credentialCache = new CredentialCache();
                credentialCache.Add(uri, "Negotiate", new NetworkCredential("[username]", "[password]", "[domain]"));
                var handler= new HttpClientHandler() {  Credentials = credentialCache, PreAuthenticate=true };
                HttpClient httpClient = new HttpClient(handler);
                httpClient.BaseAddress = uri;
                
                Result = await httpClient.GetStringAsync($"apigateway/crmapi/accounts?$select=accountid,accountnumber,address1_city,address1_country,address1_county,address1_line1,address1_line2,address1_line3,address1_name,address1_postalcode,address1_stateorprovince&$filter=accountnumber eq '{AccountNumber}'");
            }
        }
    }
}
