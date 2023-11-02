namespace FS.Keycloak.RestApiClient.Model
{
    /// <summary>
    /// Authenticate against Keycloak using client credentials flow.
    /// </summary>
    public class ClientCredentials : AuthenticationFlow
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