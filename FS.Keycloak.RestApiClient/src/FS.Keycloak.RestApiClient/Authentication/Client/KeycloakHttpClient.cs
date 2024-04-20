using FS.Keycloak.RestApiClient.Authentication.Client;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using System;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace FS.Keycloak.RestApiClient.Client
{
    /// <inheritdoc />
    [Obsolete("Use AuthenticationHttpClientFactory instead")]
    public sealed class KeycloakHttpClient : PasswordGrantHttpClient
    {
        /// <summary>
        ///  Creates a HttpClient authenticated against a Keycloak server.
        /// </summary>
        /// <param name="authServerUrl">Base URL to keycloak server, e.g. https://keycloak.example.com:8443/</param>
        /// <param name="user">Username to authenticate with.</param>
        /// <param name="password">Password for the user to authenticate.</param>
        [Obsolete("Use AuthenticationHttpClientFactory with explicit authentication flow parameter.")]
        public KeycloakHttpClient(string authServerUrl, string user, string password)
            : this(authServerUrl, user, password, new HttpClientHandler()) { }

        /// <summary>
        ///  Creates a HttpClient authenticated against a Keycloak server.
        /// </summary>
        /// <param name="authServerUrl">Base URL to keycloak server, e.g. https://keycloak.example.com:8443/</param>
        /// <param name="realm">The realm to authenticate against.</param>
        /// <param name="user">Username to authenticate with.</param>
        /// <param name="password">Password for the user to authenticate.</param>
        [Obsolete("Use AuthenticationHttpClientFactory with explicit authentication flow parameter.")]
        public KeycloakHttpClient(string authServerUrl, string realm, string user, string password)
            : this(authServerUrl, realm, user, password, new HttpClientHandler()) { }

        /// <summary>
        ///  Creates a HttpClient authenticated against a Keycloak server.
        /// </summary>
        /// <param name="authServerUrl">Base URL to keycloak server, e.g. https://keycloak.example.com:8443/</param>
        /// <param name="user">Username to authenticate with.</param>
        /// <param name="password">Password for the user to authenticate.</param>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> responsible for processing the HTTP response messages.</param>
        [Obsolete("Use AuthenticationHttpClientFactory with explicit authentication flow parameter.")]
        public KeycloakHttpClient(string authServerUrl, string user, string password, HttpMessageHandler handler)
            : this(authServerUrl, user, password, handler, true) { }

        /// <summary>
        ///  Creates a HttpClient authenticated against a Keycloak server.
        /// </summary>
        /// <param name="authServerUrl">Base URL to keycloak server, e.g. https://keycloak.example.com:8443/</param>
        /// <param name="realm">The realm to authenticate against.</param>
        /// <param name="user">Username to authenticate with.</param>
        /// <param name="password">Password for the user to authenticate.</param>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> responsible for processing the HTTP response messages.</param>
        [Obsolete("Use AuthenticationHttpClientFactory with explicit authentication flow parameter.")]
        public KeycloakHttpClient(string authServerUrl, string realm, string user, string password, HttpMessageHandler handler)
            : this(authServerUrl, realm, user, password, handler, true) { }

        /// <summary>
        ///  Creates a HttpClient authenticated against a Keycloak server.
        /// </summary>
        /// <param name="authServerUrl">Base URL to keycloak server, e.g. https://keycloak.example.com:8443/</param>
        /// <param name="user">Username to authenticate with.</param>
        /// <param name="password">Password for the user to authenticate.</param>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> responsible for processing the HTTP response messages.</param>
        /// <param name="disposeHandler"><see langword="true" /> if the inner handler should be disposed of by HttpClient.Dispose; <see langword="false" /> if you intend to reuse the inner handler.</param>
        [Obsolete("Use AuthenticationHttpClientFactory with explicit authentication flow parameter.")]
        public KeycloakHttpClient(string authServerUrl, string user, string password, HttpMessageHandler handler, bool disposeHandler)
            : this(authServerUrl, "master", user, password, handler, disposeHandler) { }

        /// <summary>
        ///  Creates a HttpClient authenticated against a Keycloak server.
        /// </summary>
        /// <param name="authServerUrl">Base URL to keycloak server, e.g. https://keycloak.example.com:8443/</param>
        /// <param name="realm">The realm to authenticate against.</param>
        /// <param name="user">Username to authenticate with.</param>
        /// <param name="password">Password for the user to authenticate.</param>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> responsible for processing the HTTP response messages.</param>
        /// <param name="disposeHandler"><see langword="true" /> if the inner handler should be disposed of by HttpClient.Dispose; <see langword="false" /> if you intend to reuse the inner handler.</param>
        [Obsolete("Use AuthenticationHttpClientFactory with explicit authentication flow parameter.")]
        public KeycloakHttpClient(string authServerUrl, string realm, string user, string password, HttpMessageHandler handler, bool disposeHandler)
            : base(GetPasswordGrantFlow(authServerUrl, realm, user, password), handler, disposeHandler)
        {
        }

        private static PasswordGrantFlow GetPasswordGrantFlow(string authServerUrl, string realm, string user, string password)
            => new PasswordGrantFlow
            {
                KeycloakUrl = authServerUrl,
                Realm = realm,
                UserName = user,
                Password = password
            };
    }
}