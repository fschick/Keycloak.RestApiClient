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
}