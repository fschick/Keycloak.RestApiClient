using FS.Keycloak.RestApiClient.Authentication.Client;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FS.Keycloak.RestApiClient.Test.Authentication
{
    /// <summary>
    /// The authenticating clients cache their token, which only pays off while one client
    /// serves more than a single request. These tests pin down that a client shared by
    /// concurrent requests asks for exactly one token instead of one per request.
    /// </summary>
    public class TokenRefreshTests
    {
        private const int ConcurrentRequests = 50;

        // Callers arriving after a failure retry by design, so the failing request must stay
        // in flight until all concurrent callers are queued behind it.
        private static readonly TimeSpan FailingTokenDelay = TimeSpan.FromMilliseconds(500);

        [Fact]
        public async Task WhenClientCredentialsClientIsUsedConcurrently_OnlyOneTokenIsRequested()
        {
            using var handler = new TokenCountingHandler();
            var flow = new ClientCredentialsFlow
            {
                KeycloakUrl = "https://keycloak.example.com",
                Realm = "test",
                ClientId = "client",
                ClientSecret = "secret",
            };

            using var client = new ClientCredentialsGrantHttpClient(flow, handler, disposeHandler: false);
            await SendConcurrently(client, flow.KeycloakUrl);

            Assert.Equal(1, handler.TokenRequestCount);
        }

        [Fact]
        public async Task WhenPasswordGrantClientIsUsedConcurrently_OnlyOneTokenIsRequested()
        {
            using var handler = new TokenCountingHandler();
            var flow = new PasswordGrantFlow
            {
                KeycloakUrl = "https://keycloak.example.com",
                Realm = "test",
                UserName = "user",
                Password = "password",
            };

            using var client = new PasswordGrantHttpClient(flow, handler, disposeHandler: false);
            await SendConcurrently(client, flow.KeycloakUrl);

            Assert.Equal(1, handler.TokenRequestCount);
        }

        [Fact]
        public async Task WhenTokenIsStillValid_NoFurtherTokenIsRequested()
        {
            using var handler = new TokenCountingHandler();
            var flow = new ClientCredentialsFlow
            {
                KeycloakUrl = "https://keycloak.example.com",
                Realm = "test",
                ClientId = "client",
                ClientSecret = "secret",
            };

            using var client = new ClientCredentialsGrantHttpClient(flow, handler, disposeHandler: false);
            await SendConcurrently(client, flow.KeycloakUrl);
            await SendConcurrently(client, flow.KeycloakUrl);

            Assert.Equal(1, handler.TokenRequestCount);
        }

        [Fact]
        public async Task WhenTokenRequestFails_WaitingCallersFailWithoutRetrying()
        {
            using var handler = new TokenCountingHandler(tokenStatusCode: HttpStatusCode.InternalServerError, tokenDelay: FailingTokenDelay);
            var flow = CreateClientCredentialsFlow();

            using var client = new ClientCredentialsGrantHttpClient(flow, handler, disposeHandler: false);
            var failures = await SendConcurrentlyExpectingFailure(client, flow.KeycloakUrl);

            Assert.Equal(ConcurrentRequests, failures);
            Assert.Equal(1, handler.TokenRequestCount);
        }

        [Fact]
        public async Task WhenTokenRequestHangs_WaitingCallersFailAfterOneTimeout()
        {
            using var handler = new TokenCountingHandler(tokenDelay: Timeout.InfiniteTimeSpan);
            var flow = CreateClientCredentialsFlow();

            using var client = new ClientCredentialsGrantHttpClient(flow, handler, disposeHandler: false);
            client.Timeout = FailingTokenDelay;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var failures = await SendConcurrentlyExpectingFailure(client, flow.KeycloakUrl);
            stopwatch.Stop();

            Assert.Equal(ConcurrentRequests, failures);
            Assert.Equal(1, handler.TokenRequestCount);
            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5), $"Callers took {stopwatch.Elapsed} to fail.");
        }

        [Fact]
        public async Task WhenTokenRequestFailed_NextCallRetries()
        {
            using var handler = new TokenCountingHandler(tokenStatusCode: HttpStatusCode.InternalServerError, tokenDelay: FailingTokenDelay);
            var flow = CreateClientCredentialsFlow();

            using var client = new ClientCredentialsGrantHttpClient(flow, handler, disposeHandler: false);
            await SendConcurrentlyExpectingFailure(client, flow.KeycloakUrl);

            handler.TokenStatusCode = HttpStatusCode.OK;
            await SendConcurrently(client, flow.KeycloakUrl);

            Assert.Equal(2, handler.TokenRequestCount);
        }

        private static ClientCredentialsFlow CreateClientCredentialsFlow()
            => new ClientCredentialsFlow
            {
                KeycloakUrl = "https://keycloak.example.com",
                Realm = "test",
                ClientId = "client",
                ClientSecret = "secret",
            };

        private static async Task<int> SendConcurrentlyExpectingFailure(HttpClient client, string keycloakUrl)
        {
            var requests = Enumerable
                .Range(0, ConcurrentRequests)
                .Select(_ => Task.Run(async () =>
                {
                    try
                    {
                        using var request = new HttpRequestMessage(HttpMethod.Get, $"{keycloakUrl}/resource");
                        using var response = await client.SendAsync(request, CancellationToken.None);
                        return false;
                    }
                    catch (Exception)
                    {
                        return true;
                    }
                }))
                .ToList();

            var results = await Task.WhenAll(requests);
            return results.Count(failed => failed);
        }

        private static async Task SendConcurrently(HttpClient client, string keycloakUrl)
        {
            var requests = Enumerable
                .Range(0, ConcurrentRequests)
                .Select(_ => Task.Run(async () =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, $"{keycloakUrl}/resource");
                    using var response = await client.SendAsync(request, CancellationToken.None);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                }))
                .ToList();

            await Task.WhenAll(requests);
        }

        /// <summary>
        /// Answers token requests with a canned token and counts them. The delay widens the
        /// window in which concurrent callers can all find the token missing or expired.
        /// </summary>
        private sealed class TokenCountingHandler : HttpMessageHandler
        {
            private readonly int _expiresInSeconds;
            private readonly TimeSpan _tokenDelay;
            private int _tokenRequestCount;

            public TokenCountingHandler(int expiresInSeconds = 300, HttpStatusCode tokenStatusCode = HttpStatusCode.OK, TimeSpan? tokenDelay = null)
            {
                _expiresInSeconds = expiresInSeconds;
                _tokenDelay = tokenDelay ?? TimeSpan.FromMilliseconds(50);
                TokenStatusCode = tokenStatusCode;
            }

            public HttpStatusCode TokenStatusCode { get; set; }

            public int TokenRequestCount => Volatile.Read(ref _tokenRequestCount);

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var isTokenRequest = request.RequestUri.AbsolutePath.EndsWith("/protocol/openid-connect/token", StringComparison.Ordinal);
                if (!isTokenRequest)
                    return new HttpResponseMessage(HttpStatusCode.OK);

                Interlocked.Increment(ref _tokenRequestCount);
                await Task.Delay(_tokenDelay, cancellationToken);

                if (TokenStatusCode != HttpStatusCode.OK)
                    return new HttpResponseMessage(TokenStatusCode);

                var token = new Dictionary<string, object>
                {
                    { "access_token", "access-token" },
                    { "expires_in", _expiresInSeconds },
                    { "refresh_expires_in", 0 },
                    { "refresh_token", "refresh-token" },
                    { "token_type", "Bearer" },
                    { "not-before-policy", 0 },
                    { "session_state", "session" },
                    { "scope", "openid" },
                };

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(token);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
            }
        }
    }
}
