using System;

namespace FS.Keycloak.RestApiClient.Authentication.Flow
{
    public abstract class AuthenticationFlow
    {
        /// <summary>
        /// Base URL to keycloak server, e.g. https://keycloak.example.com:8443/.
        /// </summary>
        public string KeycloakUrl { get; set; }

        /// <summary>
        /// Base URL to keycloak server, e.g. https://keycloak.example.com:8443/.
        /// </summary>
        [Obsolete("Use KeycloakUrl instead.")]
        public string AuthUrl { get => KeycloakUrl; set => KeycloakUrl = value; }

        private string _realm;
        public string Realm
        {
            get => _realm ?? "master";
            set => _realm = value;
        }
    }
}