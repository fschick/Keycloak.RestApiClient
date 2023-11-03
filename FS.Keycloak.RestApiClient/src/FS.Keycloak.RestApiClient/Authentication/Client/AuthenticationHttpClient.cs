using FS.Keycloak.RestApiClient.Authentication.Flow;
using FS.Keycloak.RestApiClient.Client;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace FS.Keycloak.RestApiClient.Authentication.Client
{
    /// <summary>
    /// Base class for authentication clients.
    /// </summary>
    public abstract class AuthenticationHttpClient : HttpClient
    {
        protected static readonly JsonSerializerSettings KeycloakJsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new SnakeCaseContractResolver() };

        /// <summary>
        /// Base URL to keycloak server, e.g. https://keycloak.example.com:8443/
        /// </summary>
        public string KeycloakUrl { get; }

        /// <summary>
        /// URL to keycloak server's token endpoint
        /// </summary>
        public string AuthTokenUrl { get; }

        protected AuthenticationHttpClient(AuthenticationFlow flow, HttpMessageHandler handler, bool disposeHandler)
            : base(handler ?? throw new ArgumentNullException(nameof(handler)), disposeHandler)
        {
            if (flow == null)
                throw new ArgumentNullException(nameof(flow));

            KeycloakUrl = flow.KeycloakUrl;
            AuthTokenUrl = $"{KeycloakUrl}/realms/{flow.Realm}/protocol/openid-connect/token";
        }
    }
}
