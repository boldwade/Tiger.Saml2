using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;

namespace TigerAuth.Client.Web;

public static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder UseTigerAuthentication(this IApplicationBuilder app)
    {
        app.Map("/", builder => { });
        app.UseMiddleware<AuthenticationMiddleware>();
        return app;
    }
}
