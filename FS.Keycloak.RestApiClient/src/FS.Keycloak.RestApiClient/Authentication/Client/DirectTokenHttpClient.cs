using FS.Keycloak.RestApiClient.Authentication.Client;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using FS.Keycloak.RestApiClient.Model;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FS.Keycloak.RestApiClient.Authentication.Client
{
    public class DirectTokenHttpClient : AuthenticationHttpClient
    {
        private readonly string _token;

        public DirectTokenHttpClient(DirectTokenFlow flow, HttpMessageHandler handler = null, bool disposeHandler = true)
            : base(flow, handler ?? new HttpClientHandler(), disposeHandler)
            => _token = flow.Token;

        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AddAuthorizationHeader(request);
            return await base.SendAsync(request, cancellationToken);
        }

        private void AddAuthorizationHeader(HttpRequestMessage request)
            => request.Headers.Authorization = new AuthenticationHeaderValue("bearer", _token);
    }
}

namespace FS.Keycloak.RestApiClient.Client.Auth
{
    [Obsolete("Use DirectTokenHttpClient instead.")]
    public class AuthClientDirectToken : DirectTokenHttpClient
    {
        public AuthClientDirectToken(DirectToken flow, HttpMessageHandler handler = null, bool disposeHandler = true)
            : base(flow, handler, disposeHandler) { }
    }
}
