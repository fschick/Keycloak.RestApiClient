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
        public enum AuthenticationFlow
        {
            ClientCredentials,
            PasswordGrant
        }

        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new SnakeCaseContractResolver() };

        private readonly string _authTokenUrl;
        private readonly Dictionary<string, string> _authParameters;
        private KeycloakApiToken _token;

        public string AuthServerUrl { get; }

        public KeycloakHttpClient(string authServerUrl,
            string userOrClientId, string passwordOrClientSecret, AuthenticationFlow flow = AuthenticationFlow.PasswordGrant)
            : this(authServerUrl, userOrClientId, passwordOrClientSecret, new HttpClientHandler(), flow) { }

        public KeycloakHttpClient(string authServerUrl,
            string realm, string userOrClientId, string passwordOrClientSecret, AuthenticationFlow flow = AuthenticationFlow.PasswordGrant)
            : this(authServerUrl, realm, userOrClientId, passwordOrClientSecret, new HttpClientHandler(), flow) { }

        public KeycloakHttpClient(string authServerUrl,
            string userOrClientId, string passwordOrClientSecret, HttpMessageHandler handler, AuthenticationFlow flow = AuthenticationFlow.PasswordGrant)
            : this(authServerUrl, userOrClientId, passwordOrClientSecret, handler, true, flow) { }

        public KeycloakHttpClient(string authServerUrl,
            string realm, string userOrClientId, string passwordOrClientSecret, HttpMessageHandler handler, AuthenticationFlow flow = AuthenticationFlow.PasswordGrant)
            : this(authServerUrl, realm, userOrClientId, passwordOrClientSecret, handler, true, flow) { }

        public KeycloakHttpClient(string authServerUrl,
            string userOrClientId, string passwordOrClientSecret, HttpMessageHandler handler, bool disposeHandler, AuthenticationFlow flow = AuthenticationFlow.PasswordGrant)
            : this(authServerUrl, "master", userOrClientId, passwordOrClientSecret, handler, disposeHandler, flow) { }


        public KeycloakHttpClient(string authServerUrl,
            string realm, string userOrClientId, string passwordOrClientSecret, HttpMessageHandler handler, bool disposeHandler,
            AuthenticationFlow flow = AuthenticationFlow.PasswordGrant)
            : base(handler, disposeHandler)
        {
            _authTokenUrl = $"{authServerUrl}/realms/{realm}/protocol/openid-connect/token";

            if (flow == AuthenticationFlow.PasswordGrant)
            {
                _authParameters = new Dictionary<string, string>
                {
                    { "client_id", "admin-cli" },
                    { "grant_type", "password" },
                    { "username", userOrClientId },
                    { "password", passwordOrClientSecret },
                };
            }
            else
            {
                _authParameters = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", userOrClientId },
                    { "client_secret", passwordOrClientSecret },
                };
            }

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
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _authTokenUrl) { Content = new FormUrlEncodedContent(_authParameters) };
            var response = await base.SendAsync(tokenRequest, cancellationToken); // todo handle error in authentication response
            var tokenJson = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<KeycloakApiToken>(tokenJson, _jsonSerializerSettings);
            return token;
        }
    }
}