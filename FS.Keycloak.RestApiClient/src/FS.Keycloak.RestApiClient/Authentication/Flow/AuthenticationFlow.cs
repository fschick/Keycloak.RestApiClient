using System;

namespace FS.Keycloak.RestApiClient.Authentication.Flow
{
    /// <summary>
    /// Base class for authentication flows.
    /// </summary>
    public abstract class AuthenticationFlow
    {
        private string _realm;
        private string _scope;

        /// <summary>
        /// Base URL to keycloak server, e.g. https://keycloak.example.com:8443/.
        /// </summary>
        public string KeycloakUrl { get; set; }

        /// <summary>
        /// The realm to authenticate against.
        /// </summary>
        public string Realm { get => _realm ?? "master"; set => _realm = value; }

        /// <summary>
        /// Base URL to keycloak server, e.g. https://keycloak.example.com:8443/.
        /// </summary>
        [Obsolete("Use KeycloakUrl instead.")]
        public string AuthUrl
        {
            get => KeycloakUrl;
            set => KeycloakUrl = value;
        }

        /// <summary>
        /// Space-delimited list of scopes requested by login.
        /// </summary>
        public string Scope
        {
            get => _scope ?? string.Empty;
            set => _scope = value;
        }
    }
}