using FS.Keycloak.RestApiClient.Authentication.Client;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using FS.Keycloak.RestApiClient.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FS.Keycloak.RestApiClient.Authentication.Client
{
    /// <inheritdoc />
    public class PasswordGrantHttpClient : AuthenticationHttpClient
    {
        private readonly Dictionary<string, string> _parameters;

        /// <inheritdoc />
        public PasswordGrantHttpClient(AuthenticationFlow flow)
            : base(flow) { }

        /// <inheritdoc />
        public PasswordGrantHttpClient(PasswordGrantFlow flow, HttpMessageHandler handler, bool disposeHandler)
            : base(flow, handler, disposeHandler)
        {
            _parameters = new Dictionary<string, string>
            {
                { "client_id", "admin-cli" },
                { "grant_type", "password" },
                { "username", flow.UserName },
                { "password", flow.Password },
            };

            if (!string.IsNullOrWhiteSpace(flow.Scope))
                _parameters.Add("scope", flow.Scope);
        }

        /// <inheritdoc />
        protected override async Task<KeycloakApiToken> GetToken(CancellationToken cancellationToken)
        {
            using (var tokenRequest = new HttpRequestMessage(HttpMethod.Post, AuthTokenUrl))
            {
                tokenRequest.Content = new FormUrlEncodedContent(_parameters);
                using (var response = await SendUnauthenticatedAsync(tokenRequest, cancellationToken))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception($"Username and password authentication failed with code: {response.StatusCode}");

                    var tokenJson = await response.Content.ReadAsStringAsync();
                    var token = JsonConvert.DeserializeObject<KeycloakApiToken>(tokenJson, KeycloakJsonSerializerSettings);
                    return token;
                }
            }
        }
    }
}

namespace FS.Keycloak.RestApiClient.Client.Auth
{
    /// <inheritdoc />
    [Obsolete("Use PasswordGrantHttpClient instead.")]
    public class AuthClientPasswordGrant : PasswordGrantHttpClient
    {
        /// <inheritdoc />
        public AuthClientPasswordGrant(PasswordGrant flow, HttpMessageHandler handler = null, bool disposeHandler = true)
            : base(flow, handler, disposeHandler) { }
    }
}
