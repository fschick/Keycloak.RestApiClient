using FS.Keycloak.RestApiClient.Authentication.Flow;
using System;
using System.Diagnostics.CodeAnalysis;

namespace FS.Keycloak.RestApiClient.Authentication.Flow
{
    /// <summary>
    /// Authenticate against Keycloak using a direct token.
    /// </summary>
    public class DirectTokenFlow : AuthenticationFlow
    {
        /// <summary>
        /// The token to use for authentication.
        /// </summary>
        public string Token { get; set; }
    }
}

namespace FS.Keycloak.RestApiClient.Model
{
    /// <summary>
    /// Authenticate against Keycloak using a direct token.
    /// </summary>
    [Obsolete("Use DirectTokenFlow instead.")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class DirectToken : DirectTokenFlow
    {
    }
}