using System;

namespace FS.Keycloak.RestApiClient.Client
{
    public static class ApiClientFactory
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