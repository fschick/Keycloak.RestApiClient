using FS.Keycloak.RestApiClient.Authentication.Client;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using FS.Keycloak.RestApiClient.Model;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FS.Keycloak.RestApiClient.Authentication.Client
{
    /// <inheritdoc />
    public class DirectTokenHttpClient : AuthenticationHttpClient
    {
        private readonly KeycloakApiToken _token;

        /// <inheritdoc />
        public DirectTokenHttpClient(AuthenticationFlow flow)
            : base(flow) { }

        /// <inheritdoc />
        public DirectTokenHttpClient(DirectTokenFlow flow, HttpMessageHandler handler, bool disposeHandler)
            : base(flow, handler, disposeHandler)
            => _token = new KeycloakApiToken(flow.Token, int.MaxValue, 0, null, "Bearer", 0, null, null);

        /// <inheritdoc />
        protected override Task<KeycloakApiToken> GetToken(CancellationToken cancellationToken)
            => Task.FromResult(_token);
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
