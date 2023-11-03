using FS.Keycloak.RestApiClient.Authentication.Client;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using System;
using System.Net.Http;

namespace FS.Keycloak.RestApiClient.Client
{
    [Obsolete("Use AuthenticationClientFactory instead")]
    public sealed class KeycloakHttpClient : PasswordGrantHttpClient
    {
        [Obsolete("Use AuthenticationClientFactory with explicit authentication flow parameter.")]
        public KeycloakHttpClient(string authServerUrl, string user, string password)
            : this(authServerUrl, user, password, new HttpClientHandler()) { }

        [Obsolete("Use AuthenticationClientFactory with explicit authentication flow parameter.")]
        public KeycloakHttpClient(string authServerUrl, string realm, string user, string password)
            : this(authServerUrl, realm, user, password, new HttpClientHandler()) { }

        [Obsolete("Use AuthenticationClientFactory with explicit authentication flow parameter.")]
        public KeycloakHttpClient(string authServerUrl, string user, string password, HttpMessageHandler handler)
            : this(authServerUrl, user, password, handler, true) { }

        [Obsolete("Use AuthenticationClientFactory with explicit authentication flow parameter.")]
        public KeycloakHttpClient(string authServerUrl, string realm, string user, string password, HttpMessageHandler handler)
            : this(authServerUrl, realm, user, password, handler, true) { }

        [Obsolete("Use AuthenticationClientFactory with explicit authentication flow parameter.")]
        public KeycloakHttpClient(string authServerUrl, string user, string password, HttpMessageHandler handler, bool disposeHandler)
            : this(authServerUrl, "master", user, password, handler, disposeHandler) { }

        [Obsolete("Use AuthenticationClientFactory with explicit authentication flow parameter.")]
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