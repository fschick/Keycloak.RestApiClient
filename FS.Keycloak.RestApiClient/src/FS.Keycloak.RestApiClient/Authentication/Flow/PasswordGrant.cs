namespace FS.Keycloak.RestApiClient.Model
{
    /// <summary>
    /// Authenticate against Keycloak using password grant flow.
    /// </summary>
    public class PasswordGrant : AuthenticationFlow
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