using Newtonsoft.Json.Serialization;

namespace FS.Keycloak.RestApiClient.Client
{
    internal class SnakeCaseContractResolver : DefaultContractResolver
    {
        public SnakeCaseContractResolver()
            => NamingStrategy = new SnakeCaseNamingStrategy();
    }
}