using FS.Keycloak.RestApiClient.Authentication.Flow;
using FS.Keycloak.RestApiClient.Client;
using FS.Keycloak.RestApiClient.Model;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace FS.Keycloak.RestApiClient.Authentication.Client
{
    /// <summary>
    /// Base class for authentication clients.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public abstract class AuthenticationHttpClient : HttpClient
    {
        /// <summary>
        /// JSON serializer settings for Keycloak API.
        /// </summary>
        protected static readonly JsonSerializerSettings KeycloakJsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new SnakeCaseContractResolver() };

        private KeycloakApiToken _token;
        private readonly SemaphoreSlim _tokenLock = new SemaphoreSlim(1, 1);
        private ExceptionDispatchInfo _lastRenewalError;

        /// <summary>
        /// Base URL to keycloak server, e.g. https://keycloak.example.com:8443
        /// </summary>
        public string KeycloakUrl { get; }

        /// <summary>
        /// URL to keycloak server's token endpoint
        /// </summary>
        public string AuthTokenUrl { get; }

        /// <summary>
        /// Creates a HttpClient authenticated against a Keycloak server.
        /// </summary>
        /// <param name="flow">Data used for authentication.</param>
        protected AuthenticationHttpClient(AuthenticationFlow flow)
            : this(flow, new HttpClientHandler(), true) { }

        /// <summary>
        /// Creates a HttpClient authenticated against a Keycloak server.
        /// </summary>
        /// <param name="flow">Data used for authentication.</param>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> responsible for processing the HTTP response messages.</param>
        /// <param name="disposeHandler"><see langword="true" /> if the inner handler should be disposed of by HttpClient.Dispose; <see langword="false" /> if you intend to reuse the inner handler.</param>
        protected AuthenticationHttpClient(AuthenticationFlow flow, HttpMessageHandler handler, bool disposeHandler)
            : base(handler ?? throw new ArgumentNullException(nameof(handler)), disposeHandler)
        {
            if (flow == null)
                throw new ArgumentNullException(nameof(flow));

            KeycloakUrl = flow.KeycloakUrl.Trim('/');
            AuthTokenUrl = $"{KeycloakUrl}/realms/{flow.Realm}/protocol/openid-connect/token";
        }

        /// <inheritdoc />
        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await AddAuthorizationHeader(request, cancellationToken);
            return await base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Obtains a new access token.
        /// </summary>
        protected abstract Task<KeycloakApiToken> GetToken(CancellationToken cancellationToken);

        /// <summary>
        /// Sends a request without adding the authorization header, e.g. to request a token.
        /// </summary>
        protected Task<HttpResponseMessage> SendUnauthenticatedAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => base.SendAsync(request, cancellationToken);

        private async Task AddAuthorizationHeader(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _token;
            if (token == null || token.IsExpired)
                token = await GetTokenSynchronized(cancellationToken);

            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.AccessToken);
        }

        /// <summary>
        /// Returns the cached token and renews it when needed, letting only one caller at a
        /// time perform the renewal. Without the lock every request that finds the token
        /// expired fetches one of its own, so a client shared by concurrent requests - which
        /// is what makes the cache worthwhile in the first place - produces one token request
        /// per in-flight request at every expiry instead of a single one.
        /// </summary>
        private async Task<KeycloakApiToken> GetTokenSynchronized(CancellationToken cancellationToken)
        {
            var errorBeforeWait = Volatile.Read(ref _lastRenewalError);
            await _tokenLock.WaitAsync(cancellationToken);

            try
            {
                // Re-check inside the lock: another caller may have renewed it while we waited.
                if (_token != null && !_token.IsExpired)
                    return _token;

                // A renewal failed while we waited. Retrying would queue every waiting caller
                // behind one more full timeout each, so share the failure instead. Callers
                // arriving after the failure saw that error before waiting and start a fresh attempt.
                var error = _lastRenewalError;
                if (error != null && error != errorBeforeWait)
                    error.Throw();

                try
                {
                    _token = await GetToken(cancellationToken);
                    _lastRenewalError = null;
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    Volatile.Write(ref _lastRenewalError, ExceptionDispatchInfo.Capture(ex));
                    throw;
                }

                return _token;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _tokenLock.Dispose();

            base.Dispose(disposing);
        }
    }
}
