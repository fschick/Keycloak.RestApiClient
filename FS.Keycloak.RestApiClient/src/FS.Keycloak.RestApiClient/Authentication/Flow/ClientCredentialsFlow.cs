using FS.Keycloak.RestApiClient.Authentication.Flow;
using System;

namespace FS.Keycloak.RestApiClient.Authentication.Flow
{
    /// <summary>
    /// Authenticate against Keycloak using client credentials flow.
    /// </summary>
    public class ClientCredentialsFlow : AuthenticationFlow
    {
        /// <summary>
        /// The client id of the client to authenticate.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The client secret of the client to authenticate.
        /// </summary>
        public string ClientSecret { get; set; }
    }
}

namespace FS.Keycloak.RestApiClient.Model
{
    /// <summary>
    /// Authenticate against Keycloak using client credentials flow.
    /// </summary>
    [Obsolete("Use ClientCredentialsFlow instead.")]
    public class ClientCredentials : ClientCredentialsFlow
    {
    }
}