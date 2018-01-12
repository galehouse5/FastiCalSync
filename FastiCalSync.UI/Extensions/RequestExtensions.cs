using Microsoft.AspNetCore.Http;

namespace FastiCalSync.UI.Extensions
{
    public static class RequestExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
            => "XMLHttpRequest".Equals(request.Headers["X-Requested-With"]);
    }
}
