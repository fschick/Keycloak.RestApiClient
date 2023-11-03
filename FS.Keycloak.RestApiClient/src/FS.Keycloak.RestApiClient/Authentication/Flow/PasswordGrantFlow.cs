using FS.Keycloak.RestApiClient.Authentication.Flow;
using System;

namespace FS.Keycloak.RestApiClient.Authentication.Flow
{
    /// <summary>
    /// Authenticate against Keycloak using password grant flow.
    /// </summary>
    public class PasswordGrantFlow : AuthenticationFlow
    {
        /// <summary>
        /// The username of the user to authenticate.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The password of the user to authenticate.
        /// </summary>
        public string Password { get; set; }
    }
}

namespace FS.Keycloak.RestApiClient.Model
{
    /// <summary>
    /// Authenticate against Keycloak using password grant flow.
    /// </summary>
    [Obsolete("Use PasswordGrantFlow instead.")]
    public class PasswordGrant : PasswordGrantFlow
    {
    }
}