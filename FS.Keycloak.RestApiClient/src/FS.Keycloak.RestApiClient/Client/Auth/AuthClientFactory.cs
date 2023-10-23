using FS.Keycloak.RestApiClient.Model;

namespace FS.Keycloak.RestApiClient.Client.Auth
{
    public class AuthClientFactory
    {
        public static HttpClient Create<T>(T flow, HttpMessageHandler? handler = null, bool disposeHandler = true)
        {
            if (typeof(T) == typeof(ClientCredentials))
            {
                return new AuthClientClientCredentials(flow as ClientCredentials, handler, disposeHandler);
            }
            else if (typeof(T) == typeof(PasswordGrant))
            {
                return new AuthClientPasswordGrant(flow as PasswordGrant, handler, disposeHandler);
            }
            else if (typeof(T) == typeof(DirectToken))
            {
                return new AuthClientDirectToken(flow as DirectToken, handler, disposeHandler);
            }
            else
            {
                throw new ArgumentException("Unknown authentication flow parameters");
            }
        }
    }
}
