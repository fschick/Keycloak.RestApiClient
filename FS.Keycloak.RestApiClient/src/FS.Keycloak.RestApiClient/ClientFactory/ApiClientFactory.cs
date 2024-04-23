using FS.Keycloak.RestApiClient.Authentication.Client;
using FS.Keycloak.RestApiClient.Client;
using System;

namespace FS.Keycloak.RestApiClient.ClientFactory
{
    /// <summary>
    /// Factory for creating API clients.
    /// </summary>
    public class ApiClientFactory
    {
        /// <summary>
        /// Create an API client.
        /// </summary>
        /// <typeparam name="TApiClient">The type of the client.</typeparam>
        /// <param name="httpClient">The HTTP client to use.</param>
        public static TApiClient Create<TApiClient>(AuthenticationHttpClient httpClient) where TApiClient : IApiAccessor
            => (TApiClient)Activator
                .CreateInstance(
                    typeof(TApiClient),
                    httpClient,
                    new Configuration { BasePath = $"{httpClient.KeycloakUrl}" },
                    null
                );
    }
}

namespace FS.Keycloak.RestApiClient.Client
{
    /// <inheritdoc />
    [Obsolete("Use FS.Keycloak.RestApiClient.ClientFactory.ApiClientFactory instead.")]
    public class ApiClientFactory : ClientFactory.ApiClientFactory
    { }
}