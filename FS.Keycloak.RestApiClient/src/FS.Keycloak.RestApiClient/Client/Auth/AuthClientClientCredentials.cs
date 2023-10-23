using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FS.Keycloak.RestApiClient.Model;
using FS.Keycloak.RestApiClient.Client;

namespace FS.Keycloak.RestApiClient.Client.Auth
{
    public class AuthClientClientCredentials : HttpClient
    {
        private KeycloakApiToken _token;
        private readonly string _authTokenUrl;
        private readonly Dictionary<string, string> _parameters;
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new SnakeCaseContractResolver() };

        public AuthClientClientCredentials(ClientCredentials flow,
           HttpMessageHandler handler = null, bool disposeHandler = true) : base(handler ?? new HttpClientHandler(), disposeHandler)
        {
            _authTokenUrl = $"{flow.AuthUrl}/realms/{flow.Realm}/protocol/openid-connect/token";
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
            _ = _parameters ?? throw new ArgumentException("Client credentials authentication parameters cannot be null");

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _authTokenUrl) { Content = new FormUrlEncodedContent(_parameters) };
            var response = await base.SendAsync(tokenRequest, cancellationToken);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Client credentials authentication failed with code: {response.StatusCode}");
            }

            var tokenJson = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<KeycloakApiToken>(tokenJson, _jsonSerializerSettings);
            return token;
        }
    }
}

