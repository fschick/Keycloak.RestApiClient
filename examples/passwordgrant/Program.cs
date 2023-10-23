using System;
using System.Threading.Tasks;
using FS.Keycloak.RestApiClient.Client.Auth;
using FS.Keycloak.RestApiClient.Model;

internal class Program
{
    private static void Main(string[] args)
    {
        MainAsync().Wait();

        Console.WriteLine("Successfully authenticated with username and password credentials");
    }

    static async Task MainAsync()
    {
        var authClient = AuthClientFactory.Create(new PasswordGrant
        {
            AuthUrl = "https://<keycloak-url>/auth",
            Realm = "<realm>",
            UserName = "<username>",
            Password = "<password>"
        });
    }
}