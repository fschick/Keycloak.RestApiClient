using FS.Keycloak.RestApiClient.Authentication.Client;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using FS.Keycloak.RestApiClient.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;


namespace FS.Keycloak.RestApiClient.Authentication.Client
{
    public class ClientCredentialsGrantHttpClient : AuthenticationHttpClient
    {
        private KeycloakApiToken _token;
        private readonly Dictionary<string, string> _parameters;

        public ClientCredentialsGrantHttpClient(ClientCredentialsFlow flow, HttpMessageHandler handler = null, bool disposeHandler = true)
            : base(flow, handler ?? new HttpClientHandler(), disposeHandler)
        {
            _parameters = new Dictionary<string, string>
            {
                    { "grant_type", "client_credentials" },
                    { "client_id", flow.ClientId },
                    { "client_secret", flow.ClientSecret },
            };
        }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await AddAuthorizationHeader(request, cancellationToken);
            return await base.SendAsync(request, cancellationToken);
        }

        private async Task AddAuthorizationHeader(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_token == null || _token.IsExpired)
                _token = await GetToken(cancellationToken);

            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", _token.AccessToken);
        }

        private async Task<KeycloakApiToken> GetToken(CancellationToken cancellationToken)
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, AuthTokenUrl) { Content = new FormUrlEncodedContent(_parameters) };
            var response = await base.SendAsync(tokenRequest, cancellationToken);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Client credentials authentication failed with code: {response.StatusCode}");

            var tokenJson = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<KeycloakApiToken>(tokenJson, KeycloakJsonSerializerSettings);
            return token;
        }
    }
}

namespace FS.Keycloak.RestApiClient.Client.Auth
{
    [Obsolete("Use ClientCredentialsGrantHttpClient instead.")]
    public class AuthClientClientCredentials : ClientCredentialsGrantHttpClient
    {
        public AuthClientClientCredentials(ClientCredentials flow, HttpMessageHandler handler = null, bool disposeHandler = true)
            : base(flow, handler, disposeHandler) { }
    }
}
