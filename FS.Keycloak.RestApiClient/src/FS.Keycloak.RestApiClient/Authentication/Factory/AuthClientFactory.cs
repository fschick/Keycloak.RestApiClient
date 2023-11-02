using FS.Keycloak.RestApiClient.Model;
using System;
using System.Net.Http;

namespace FS.Keycloak.RestApiClient.Client.Auth
{
    public class AuthClientFactory
    {
        public static HttpClient Create<TAuthenticationFlow>(TAuthenticationFlow authenticationFlow, HttpMessageHandler handler = null, bool disposeHandler = true)
            where TAuthenticationFlow : AuthenticationFlow
        {
            switch (authenticationFlow)
            {
                case ClientCredentials clientCredentials:
                    return new AuthClientClientCredentials(clientCredentials, handler, disposeHandler);
                case PasswordGrant passwordGrant:
                    return new AuthClientPasswordGrant(passwordGrant, handler, disposeHandler);
                case DirectToken directToken:
                    return new AuthClientDirectToken(directToken, handler, disposeHandler);
                default:
                    throw new ArgumentException("Unknown authentication flow parameters");
            }
        }
    }
}
