using FS.Keycloak.RestApiClient.Client;
using System;

namespace FS.Keycloak.RestApiClient.ClientFactory
{
    public class ApiClientFactory
    {
        public static TApiClient Create<TApiClient>(KeycloakHttpClient httpClient) where TApiClient : IApiAccessor
            => (TApiClient)Activator
                .CreateInstance(
                    typeof(TApiClient),
                    httpClient,
                    new Configuration { BasePath = $"{httpClient.AuthServerUrl}/admin/realms" },
                    null
                );
    }
}

namespace FS.Keycloak.RestApiClient.Client
{
    [Obsolete("Use FS.Keycloak.RestApiClient.ClientFactory.ApiClientFactory instead.")]
    public class ApiClientFactory : ClientFactory.ApiClientFactory
    { }
}