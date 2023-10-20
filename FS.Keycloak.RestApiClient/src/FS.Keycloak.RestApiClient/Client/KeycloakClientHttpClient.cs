using FS.Keycloak.RestApiClient.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FS.Keycloak.RestApiClient.Client
{
    public sealed class KeycloakClientHttpClient : HttpClient
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new SnakeCaseContractResolver() };
        private readonly string _authTokenUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private KeycloakApiToken _token;

        public string AuthServerUrl { get; }


        public KeycloakClientHttpClient(string authServerUrl, string clientId, string clientSecret)
            : this(authServerUrl, clientId, clientSecret, new HttpClientHandler()) { }

        public KeycloakClientHttpClient(string authServerUrl, string realm, string clientId, string clientSecret)
            : this(authServerUrl, realm, clientId, clientSecret, new HttpClientHandler()) { }

        public KeycloakClientHttpClient(string authServerUrl, string clientId, string clientSecret, HttpMessageHandler handler)
            : this(authServerUrl, clientId, clientSecret, handler, true) { }

        public KeycloakClientHttpClient(string authServerUrl, string realm, string clientId, string clientSecret, HttpMessageHandler handler)
            : this(authServerUrl, realm, clientId, clientSecret, handler, true) { }

        public KeycloakClientHttpClient(string authServerUrl, string clientId, string clientSecret, HttpMessageHandler handler, bool disposeHandler)
            : this(authServerUrl, "master", clientId, clientSecret, handler, disposeHandler) { }

        public KeycloakClientHttpClient(string authServerUrl, string realm, string clientId, string clientSecret, HttpMessageHandler handler, bool disposeHandler)
            : base(handler, disposeHandler)
        {
            _authTokenUrl = $"{authServerUrl}/realms/{realm}/protocol/openid-connect/token";
            _clientId = clientId;
            _clientSecret = clientSecret;

            AuthServerUrl = authServerUrl;
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
            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
            };

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _authTokenUrl) { Content = new FormUrlEncodedContent(parameters) };
            var response = await base.SendAsync(tokenRequest, cancellationToken);
            var tokenJson = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<KeycloakApiToken>(tokenJson, _jsonSerializerSettings);
            return token;
        }
    }
}