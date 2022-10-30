using Newtonsoft.Json;
using System;

namespace FS.Keycloak.RestApiClient.Model
{
    public class KeycloakApiToken
    {
        private readonly DateTime _creationTime;

        public KeycloakApiToken(
            string accessToken,
            int expiresIn,
            int refreshExpiresIn,
            string refreshToken,
            string tokenType, int notBeforePolicy,
            string sessionState,
            string scope
        )
        {
            _creationTime = DateTime.UtcNow;
            AccessToken = accessToken;
            ExpiresIn = TimeSpan.FromSeconds(expiresIn - 10);
            RefreshExpiresIn = refreshExpiresIn;
            RefreshToken = refreshToken;
            TokenType = tokenType;
            NotBeforePolicy = notBeforePolicy;
            SessionState = sessionState;
            Scope = scope;
        }

        public bool IsExpired => DateTime.UtcNow - _creationTime > ExpiresIn;
        public string AccessToken { get; }
        public TimeSpan ExpiresIn { get; }
        public int RefreshExpiresIn { get; }
        public string RefreshToken { get; }
        public string TokenType { get; }

        [JsonProperty("not-before-policy")]
        public int NotBeforePolicy { get; }

        public string SessionState { get; }
        public string Scope { get; }
    }
}