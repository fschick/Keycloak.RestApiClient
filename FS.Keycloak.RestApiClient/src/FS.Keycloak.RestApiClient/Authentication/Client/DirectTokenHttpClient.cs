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
    /// <inheritdoc />
    public class DirectTokenHttpClient : AuthenticationHttpClient
    {
        private readonly string _token;

        /// <inheritdoc />
        public DirectTokenHttpClient(AuthenticationFlow flow)
            : base(flow) { }

        /// <inheritdoc />
        public DirectTokenHttpClient(DirectTokenFlow flow, HttpMessageHandler handler, bool disposeHandler)
            : base(flow, handler, disposeHandler)
            => _token = flow.Token;

        /// <inheritdoc />
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
    /// <inheritdoc />
    [Obsolete("Use DirectTokenHttpClient instead.")]
    public class AuthClientDirectToken : DirectTokenHttpClient
    {
        /// <inheritdoc />
        public AuthClientDirectToken(DirectToken flow, HttpMessageHandler handler = null, bool disposeHandler = true)
            : base(flow, handler, disposeHandler) { }
    }
}
