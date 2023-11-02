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
            if (typeof(TAuthenticationFlow) == typeof(ClientCredentials))
            {
                return new AuthClientClientCredentials(authenticationFlow as ClientCredentials, handler, disposeHandler);
            }
            else if (typeof(TAuthenticationFlow) == typeof(PasswordGrant))
            {
                return new AuthClientPasswordGrant(authenticationFlow as PasswordGrant, handler, disposeHandler);
            }
            else if (typeof(TAuthenticationFlow) == typeof(DirectToken))
            {
                return new AuthClientDirectToken(authenticationFlow as DirectToken, handler, disposeHandler);
            }
            else
            {
                throw new ArgumentException("Unknown authentication flow parameters");
            }
        }
    }
}
