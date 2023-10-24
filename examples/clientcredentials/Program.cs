using System;
using System.Threading.Tasks;
using FS.Keycloak.RestApiClient.Client.Auth;
using FS.Keycloak.RestApiClient.Model;

internal class Program
{
    private static void Main(string[] args)
    {
        MainAsync().Wait();

        Console.WriteLine("Successfully authenticated with client credentials");
    }

    static async Task MainAsync()
    {
        var authClient = AuthClientFactory.Create(new ClientCredentials
        {
            AuthUrl = "https://<keycloak-url>/auth",
            Realm = "<realm>",
            ClientId = "<clientid>",
            ClientSecret = "<client-secret>"
        });
    }
}