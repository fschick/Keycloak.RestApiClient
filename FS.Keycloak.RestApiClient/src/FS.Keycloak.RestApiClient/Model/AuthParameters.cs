namespace FS.Keycloak.RestApiClient.Model
{
    public abstract class AuthenticationFlow
    {
        public string AuthUrl { get; set; }
        private string _realm;

        public string Realm
        {
            get => _realm ?? "master";
            set => _realm = value;
        }
    }

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