using FS.Keycloak.RestApiClient.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FS.Keycloak.RestApiClient.Client
{
    public sealed class KeycloakHttpClient : HttpClient
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new SnakeCaseContractResolver() };
        private readonly string _authTokenUrl;
        private readonly string _user;
        private readonly string _password;
        private KeycloakApiToken _token;

        public string AuthServerUrl { get; }

        public KeycloakHttpClient(string authServerUrl, string user, string password)
            : this(authServerUrl, user, password, new HttpClientHandler()) { }

        public KeycloakHttpClient(string authServerUrl, string realm, string user, string password)
            : this(authServerUrl, realm, user, password, new HttpClientHandler()) { }

        public KeycloakHttpClient(string authServerUrl, string user, string password, HttpMessageHandler handler)
            : this(authServerUrl, user, password, handler, true) { }

        public KeycloakHttpClient(string authServerUrl, string realm, string user, string password, HttpMessageHandler handler)
            : this(authServerUrl, realm, user, password, handler, true) { }

        public KeycloakHttpClient(string authServerUrl, string user, string password, HttpMessageHandler handler, bool disposeHandler)
            : this(authServerUrl, "master", user, password, handler, disposeHandler) { }

        public KeycloakHttpClient(string authServerUrl, string realm, string user, string password, HttpMessageHandler handler, bool disposeHandler)
            : base(handler, disposeHandler)
        {
            _authTokenUrl = $"{authServerUrl}/realms/{realm}/protocol/openid-connect/token";
            _user = user;
            _password = password;

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
                { "client_id", "admin-cli" },
                { "grant_type", "password" },
                { "username", _user },
                { "password", _password },
            };

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _authTokenUrl) { Content = new FormUrlEncodedContent(parameters) };
            var response = await base.SendAsync(tokenRequest, cancellationToken);
            var tokenJson = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<KeycloakApiToken>(tokenJson, _jsonSerializerSettings);
            return token;
        }
    }
}