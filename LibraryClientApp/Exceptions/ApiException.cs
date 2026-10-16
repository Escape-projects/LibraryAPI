using System.Net;

namespace LibraryClientApp.Exceptions
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? ResponseBody { get; }

        public ApiException(HttpStatusCode statusCode, string? body) : base($"API error: {statusCode}")
        {
            StatusCode = statusCode;
            ResponseBody = body;
        }
    }
}
