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

    public class PasswordGrant : AuthenticationFlow
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }

    public class ClientCredentials : AuthenticationFlow
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }

    public class DirectToken : AuthenticationFlow
    {
        public string Token { get; set; }
    }
}