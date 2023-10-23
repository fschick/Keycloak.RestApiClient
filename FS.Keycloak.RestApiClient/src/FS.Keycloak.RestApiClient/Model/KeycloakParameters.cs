using Newtonsoft.Json;
using System;

namespace FS.Keycloak.RestApiClient.Model

{
    public class BaseFlow
    {
        public string AuthUrl { get; set; }
    }

    public class PasswordGrantFlow: BaseFlow
    {

        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class ClientCredentialsFlow: BaseFlow
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class OfflineToken: BaseFlow
    {
        public string Token { get; set; }
    }
}