using System.Security.Claims;

namespace EShoppingZone.Order.API.Handlers
{
    public class AuthorizationHeaderForwardingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationHeaderForwardingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var authorizationHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    request.Headers.Add("Authorization", authorizationHeader);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
