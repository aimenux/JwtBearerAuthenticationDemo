namespace Example06.Presentation.Authentication;

public static class HttpResults
{
    public static IResult Unauthorized(string message) => new UnauthorizedHttpResultWithResponseBody(message);

    internal class UnauthorizedHttpResultWithResponseBody : IResult, IStatusCodeHttpResult
    {
        private readonly string _responseBody;
    
        private const int UnauthorizedStatusCode = 401;
    
        public UnauthorizedHttpResultWithResponseBody(string responseBody = null)
        {
            _responseBody = responseBody;
        }

        public int? StatusCode => UnauthorizedStatusCode;

        public async Task ExecuteAsync(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
            httpContext.Response.Headers.WWWAuthenticate = JwtConstants.BearerScheme;
            httpContext.Response.StatusCode = UnauthorizedStatusCode;
            if (!string.IsNullOrWhiteSpace(_responseBody))
            {
                await httpContext.Response.WriteAsync(_responseBody);
            }
        }
    }
}