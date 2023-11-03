using FS.Keycloak.RestApiClient.Authentication.ClientFactory;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FS.Keycloak.RestApiClient.Test.Authentication
{
    public class AuthenticationTests
    {
        [Theory]
        [ClassData(typeof(AuthenticationTestDataGenerator))]
        public async Task WhenClientAuthenticationIsProvided_UserInfoCanBeRequested(AuthenticationTestData authenticationTestData)
        {
            if (authenticationTestData == null)
                return;

            if (authenticationTestData.ClientCredentialsFlow != null)
                await TestClientCredentialsFlow(authenticationTestData);

            if (authenticationTestData.PasswordGrantFlow != null)
                await TestPasswordGrantFlow(authenticationTestData);
        }

        private static async Task TestClientCredentialsFlow(AuthenticationTestData authenticationTestData)
        {
            var url = authenticationTestData.KeycloakUrl;
            var realm = authenticationTestData.KeycloakRealm;

            var clientCredentials = new ClientCredentialsFlow
            {
                KeycloakUrl = url,
                Realm = realm,
                ClientId = authenticationTestData.ClientCredentialsFlow.ClientId,
                ClientSecret = authenticationTestData.ClientCredentialsFlow.ClientSecret,
                Scope = "openid",
            };

            using var httpClient = AuthenticationHttpClientFactory.Create(clientCredentials);
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{url}/realms/{realm}/protocol/openid-connect/userinfo");
            using var response = await httpClient.SendAsync(request, CancellationToken.None);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        private static async Task TestPasswordGrantFlow(AuthenticationTestData authenticationTestData)
        {
            var url = authenticationTestData.KeycloakUrl;
            var realm = authenticationTestData.KeycloakRealm;

            var clientCredentials = new PasswordGrantFlow
            {
                KeycloakUrl = url,
                Realm = realm,
                UserName = authenticationTestData.PasswordGrantFlow.Username,
                Password = authenticationTestData.PasswordGrantFlow.Password,
                Scope = "openid",
            };

            using var httpClient = AuthenticationHttpClientFactory.Create(clientCredentials);
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{url}/realms/{realm}/protocol/openid-connect/userinfo");
            using var response = await httpClient.SendAsync(request, CancellationToken.None);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }

    public class AuthenticationTestData
    {
        public string KeycloakUrl { get; set; }

        public string KeycloakRealm { get; set; }

        public ClientCredentialsTestData ClientCredentialsFlow { get; set; }

        public PasswordGrantTestData PasswordGrantFlow { get; set; }

        public class ClientCredentialsTestData
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }

        public class PasswordGrantTestData
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }

    public class AuthenticationTestDataGenerator : IEnumerable<object[]>
    {
        private readonly IEnumerable<object[]> _data = GetTestData();

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static IEnumerable<object[]> GetTestData()
        {
            const string testDataFilename = "Authentication/AuthenticationTests.data.json";
            if (!File.Exists(testDataFilename))
                return new List<object[]> { new object[] { null } };

            var testDataJson = File.ReadAllText(testDataFilename);
            var testDataList = JsonConvert.DeserializeObject<List<AuthenticationTestData>>(testDataJson);
            return testDataList.Select(testData => new[] { testData }).ToList();
        }
    }
}
