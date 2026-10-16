using LibraryClientApp.Models;
using LibraryClientApp.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;

namespace LibraryClientApp.Middleware
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly AppStorage storage;
        private readonly TokenFileStorage tokenFile;
        private readonly IServiceProvider provider;

        public AuthHeaderHandler(AppStorage appStorage, TokenFileStorage tokenFileStorage, IServiceProvider serviceProvider)
        {
            storage = appStorage;
            tokenFile = tokenFileStorage;
            provider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? "";

            if (path.Contains("/api/auth/login") || path.Contains("/api/auth/refresh") || path.Contains("/api/auth/register"))
                return await base.SendAsync(request, cancellationToken);

            if (!string.IsNullOrEmpty(storage.AccessToken)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", storage.AccessToken);
            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var newAccessToken = await TryRefreshAsync();
                if (newAccessToken != null)
                {
                    var clone = await CloneRequestAsync(request);
                    clone.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
                    response = await base.SendAsync(clone, cancellationToken);
                }
            }

            return response;
        }

        private async Task<string?> TryRefreshAsync()
        {
            var refreshToken = tokenFile.Load();
            if (string.IsNullOrEmpty(refreshToken)) return null;

            try
            {
                var apiClient = provider.GetRequiredService<ApiClient>();
                var auth = await apiClient.RefreshAsync(refreshToken);

                storage.AccessToken = auth.accessToken;
                tokenFile.Save(auth.refreshToken);
                return auth.accessToken;
            }
            catch
            {
                tokenFile.Clear();
                storage.AccessToken = null;
                return null;
            }
        }

        private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            if (request.Content != null)
            {
                var body = await request.Content.ReadAsByteArrayAsync();
                clone.Content = new ByteArrayContent(body);
                foreach (var header in request.Content.Headers) clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            foreach (var header in request.Headers) clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return clone;
        }
    }
}
