namespace FS.Keycloak.RestApiClient.Model
{
    /// <summary>
    /// Authenticate against Keycloak using a direct token.
    /// </summary>
    public class DirectToken : AuthenticationFlow
    {
        /// <summary>
        /// The token to use for authentication.
        /// </summary>
        public string Token { get; set; }
    }
}