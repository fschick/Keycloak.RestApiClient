using FS.Keycloak.RestApiClient.Authentication.Client;
using FS.Keycloak.RestApiClient.Authentication.ClientFactory;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace FS.Keycloak.RestApiClient.Authentication.ClientFactory
{
    /// <summary>
    /// Factory to create an authenticated HttpClient used to communicate with Keycloak server.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class AuthenticationHttpClientFactory
    {
        /// <summary>
        /// Creates a HttpClient authenticated against a Keycloak server.
        /// </summary>
        /// <typeparam name="TAuthenticationFlow">The type of the authentication flow.</typeparam>
        /// <param name="authenticationFlow">Data used for authentication.</param>
        public static AuthenticationHttpClient Create<TAuthenticationFlow>(TAuthenticationFlow authenticationFlow)
            where TAuthenticationFlow : AuthenticationFlow
            => Create(authenticationFlow, new HttpClientHandler(), true);

        /// <summary>
        /// Creates a HttpClient authenticated against a Keycloak server.
        /// </summary>
        /// <typeparam name="TAuthenticationFlow">The type of the authentication flow.</typeparam>
        /// <param name="authenticationFlow">Data used for authentication.</param>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> responsible for processing the HTTP response messages.</param>
        /// <param name="disposeHandler"><see langword="true" /> if the inner handler should be disposed of by HttpClient.Dispose; <see langword="false" /> if you intend to reuse the inner handler.</param>
        public static AuthenticationHttpClient Create<TAuthenticationFlow>(TAuthenticationFlow authenticationFlow, HttpMessageHandler handler, bool disposeHandler)
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
    /// <inheritdoc />
    [Obsolete("Use AuthenticationHttpClientFactory instead")]
    public class AuthClientFactory : AuthenticationHttpClientFactory
    {
    }
}

