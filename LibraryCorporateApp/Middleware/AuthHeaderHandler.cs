using LibraryCorporateApp.Models;
using System.Net.Http.Headers;

namespace LibraryCorporateApp.Middleware
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly AppStorage storage;

        public AuthHeaderHandler(AppStorage appStorage)
        {
            storage = appStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(storage.AccessToken)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", storage.AccessToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
