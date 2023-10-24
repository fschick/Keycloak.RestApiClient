using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using FS.Keycloak.RestApiClient.Model;

namespace FS.Keycloak.RestApiClient.Client.Auth
{
    public class AuthClientDirectToken : HttpClient
    {
        private string _token;

        public AuthClientDirectToken(DirectToken flow,
           HttpMessageHandler handler = null, bool disposeHandler = true) : base(handler ?? new HttpClientHandler(), disposeHandler)
        {
            _token = flow.Token;
        }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AddAuthorizationHeader(request);
            return await base.SendAsync(request, cancellationToken);
        }

        private void AddAuthorizationHeader(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", _token);
        }
    }
}
