using Newtonsoft.Json;
using System;

namespace FS.Keycloak.RestApiClient.Model

{
    public class BaseFlow
    {
        public string AuthUrl { get; set; }
        private string _realm;

        public string Realm
        {
            get => _realm ?? "master";
            set { _realm = value; }
        }
    }

    public class PasswordGrant : BaseFlow
    {

        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class ClientCredentials : BaseFlow
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class DirectToken : BaseFlow
    {
        public string Token { get; set; }
    }
}