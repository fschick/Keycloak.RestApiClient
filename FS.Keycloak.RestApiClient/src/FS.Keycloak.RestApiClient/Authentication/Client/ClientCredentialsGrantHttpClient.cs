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
    /// <inheritdoc />
    public class ClientCredentialsGrantHttpClient : AuthenticationHttpClient
    {
        private KeycloakApiToken _token;
        private readonly SemaphoreSlim _tokenLock = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, string> _parameters;

        /// <inheritdoc />
        public ClientCredentialsGrantHttpClient(AuthenticationFlow flow)
            : base(flow) { }

        /// <inheritdoc />
        public ClientCredentialsGrantHttpClient(ClientCredentialsFlow flow, HttpMessageHandler handler, bool disposeHandler)
            : base(flow, handler, disposeHandler)
        {
            _parameters = new Dictionary<string, string>
            {
                    { "grant_type", "client_credentials" },
                    { "client_id", flow.ClientId },
                    { "client_secret", flow.ClientSecret },
            };

            if (!string.IsNullOrWhiteSpace(flow.Scope))
                _parameters.Add("scope", flow.Scope);
        }

        /// <inheritdoc />
        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await AddAuthorizationHeader(request, cancellationToken);
            return await base.SendAsync(request, cancellationToken);
        }

        private async Task AddAuthorizationHeader(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _token;
            if (token == null || token.IsExpired)
                token = await GetValidToken(cancellationToken);

            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.AccessToken);
        }

        /// <summary>
        /// Returns the cached token and renews it when needed, letting only one caller at a
        /// time perform the renewal. Without the lock every request that finds the token
        /// expired fetches one of its own, so a client shared by concurrent requests - which
        /// is what makes the cache worthwhile in the first place - produces one token request
        /// per in-flight request at every expiry instead of a single one.
        /// </summary>
        private async Task<KeycloakApiToken> GetValidToken(CancellationToken cancellationToken)
        {
            await _tokenLock.WaitAsync(cancellationToken);

            try
            {
                // Re-check inside the lock: another caller may have renewed it while we waited.
                if (_token == null || _token.IsExpired)
                    _token = await GetToken(cancellationToken);

                return _token;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        private async Task<KeycloakApiToken> GetToken(CancellationToken cancellationToken)
        {
            using (var tokenRequest = new HttpRequestMessage(HttpMethod.Post, AuthTokenUrl))
            {
                tokenRequest.Content = new FormUrlEncodedContent(_parameters);
                using (var response = await base.SendAsync(tokenRequest, cancellationToken))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception($"Client credentials authentication failed with code: {response.StatusCode}");

                    var tokenJson = await response.Content.ReadAsStringAsync();
                    var token = JsonConvert.DeserializeObject<KeycloakApiToken>(tokenJson, KeycloakJsonSerializerSettings);
                    return token;
                }
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _tokenLock.Dispose();

            base.Dispose(disposing);
        }
    }
}

namespace FS.Keycloak.RestApiClient.Client.Auth
{
    /// <inheritdoc />
    [Obsolete("Use ClientCredentialsGrantHttpClient instead.")]
    public class AuthClientClientCredentials : ClientCredentialsGrantHttpClient
    {
        /// <inheritdoc />
        public AuthClientClientCredentials(ClientCredentials flow, HttpMessageHandler handler = null, bool disposeHandler = true)
            : base(flow, handler, disposeHandler) { }
    }
}
