using FS.Keycloak.RestApiClient.Authentication.Flow;
using System;
using System.Diagnostics.CodeAnalysis;

namespace FS.Keycloak.RestApiClient.Authentication.Flow
{
    /// <summary>
    /// Authenticate against Keycloak using password grant flow.
    /// </summary>
    public class PasswordGrantFlow : AuthenticationFlow
    {
        /// <summary>
        /// Username to authenticate with.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password for the user to authenticate.
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
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class PasswordGrant : PasswordGrantFlow
    {
    }
}