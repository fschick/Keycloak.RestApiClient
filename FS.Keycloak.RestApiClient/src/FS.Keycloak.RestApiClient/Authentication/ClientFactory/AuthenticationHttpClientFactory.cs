using FS.Keycloak.RestApiClient.Authentication.Client;
using FS.Keycloak.RestApiClient.Authentication.ClientFactory;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using System;
using System.Net.Http;

namespace FS.Keycloak.RestApiClient.Authentication.ClientFactory
{
    public class AuthenticationHttpClientFactory
    {
        public static HttpClient Create<TAuthenticationFlow>(TAuthenticationFlow authenticationFlow, HttpMessageHandler handler = null, bool disposeHandler = true)
            where TAuthenticationFlow : AuthenticationFlow
        {
            switch (authenticationFlow)
            {
                case ClientCredentialsFlow clientCredentials:
                    return new ClientCredentialsGrantHttpClient(clientCredentials, handler, disposeHandler);
                case PasswordGrantFlow passwordGrant:
                    return new PasswordGrantHttpClient(passwordGrant, handler, disposeHandler);
                case DirectTokenFlow directToken:
                    return new DirectTokenHttpClient(directToken, handler, disposeHandler);
                default:
                    throw new ArgumentException("Unknown authentication flow parameters");
            }
        }
    }
}

namespace FS.Keycloak.RestApiClient.Client.Auth
{
    [Obsolete("Use AuthenticationHttpClientFactory instead")]
    public class AuthClientFactory : AuthenticationHttpClientFactory
    {
    }
}

