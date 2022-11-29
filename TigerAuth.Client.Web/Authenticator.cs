using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Sustainsys.Saml2.AspNetCore2;

namespace TigerAuth.Client.Web;

public class Authenticator : IDisposable
{
    private bool _isDisposed;
    private AuthenticationOptions? _currentOptions;
    private readonly IConfiguration _configuration;
    private readonly IDisposable? _optionsReloadToken;
    private readonly IWebHostEnvironment _environment;

    public Authenticator(
        IOptionsMonitor<AuthenticationOptions> options,
        IConfiguration configuration,
        IWebHostEnvironment environment
    )
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));
        _configuration = configuration;
        _environment = environment;
        ReloadOptions(options.CurrentValue);
        _optionsReloadToken = options.OnChange(ReloadOptions);
    }

    private void ReloadOptions(AuthenticationOptions reloadedOptions)
    {
        _currentOptions = reloadedOptions;
    }

    public Task<AuthenticationResult?> AuthenticateAsync(HttpContext http) => AuthenticateAsync(http, null);

    private async Task<AuthenticationResult?> AuthenticateAsync(HttpContext httpContext, Uri? returnUri, bool reauthenticate = false)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        var request = httpContext.Request;
        var requestUri = new Uri(request.GetDisplayUrl());
        if (returnUri == null)
        {
            returnUri = request.Query["returnUrl"].ToString()?.Length > 0
                        && Uri.TryCreate(requestUri, request.Query["returnUrl"], out var uri)
                ? uri
                : null;
        }

        // TODO: AuthenticationResult.Success == null, shouldn't we be returning an actual object that represents success instead ?
        var success = httpContext.Request.Path.Value == Routes.Return
            ? new AuthenticationResult(returnUri)
            : AuthenticationResult.Success;

        var code = (request.HasFormContentType ? request.Form["code"].FirstOrDefault() : null) ?? request.Query["code"].FirstOrDefault();

        if (string.IsNullOrEmpty(code) && !reauthenticate)
        {
            if (httpContext.User.Identities.Any(identity => identity.IsAuthenticated))
            {
                return AuthenticationResult.Success;
            }

            // var result = await _authenticationStore.RetrieveAsync(httpContext).ConfigureAwait(false);
            // if (result != null && result.Succeeded)
            // {
            //     return AuthenticationResult.Success;
            // }
        }

        var redirectUri = new Uri(requestUri, request.PathBase.Value?.TrimEnd('/') + Routes.Return + "?returnUrl=" + Uri.EscapeDataString((returnUri ?? requestUri).ToString()));
        if (returnUri?.Scheme == Uri.UriSchemeHttps && redirectUri.Scheme != Uri.UriSchemeHttps)
        {
            redirectUri = new UriBuilder(redirectUri) { Scheme = Uri.UriSchemeHttps, Port = returnUri.Port }.Uri;
        }

        if (string.IsNullOrEmpty(code))
        {
            if (_currentOptions?.ResourceOwnerType == ResourceOwnerType.AzureAdSaml)
            {
                // var authUrl = _configuration["SamlSingleSignonUrl"]
                // TODO: Get azureLoginUrl from metadata somehow? Or should we issue a challenge? test
                await httpContext.ChallengeAsync(
                    Saml2Defaults.Scheme,
                    new AuthenticationProperties { RedirectUri = "https://localhost:44405" }
                ).ConfigureAwait(false);

                var location = httpContext.Response.Headers.Location;
                httpContext.Response.Headers.Remove("location");
                //httpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                return new AuthenticationResult(new Uri(location));
            }

            // var authUrl = await _accessTokenClient.GetAuthorizationUriAsync(
            //     redirectUri, string.Empty,
            //     scope: _currentOptions?.Scope,
            //     resourceOwnerType: _currentOptions?.ResourceOwnerType ?? ResourceOwnerType.DomainUser,
            //     environment: _currentOptions?.EnvironmentName ?? _configuration["TigerEnvironment"] ?? _environment.EnvironmentName,
            //     version: _currentOptions?.ApplicationVersion ?? _version
            // ).ConfigureAwait(false);
            // return new AuthenticationResult(authUrl);
            return new AuthenticationResult(null);
        }

        if (httpContext.User.Identities.Any(identity => identity.IsAuthenticated))
        {
            await httpContext.SignOutAsync().ConfigureAwait(false);
        }

        // var token = await _accessTokenClient.RequestByCodeAsync(code, redirectUri).ConfigureAwait(false);
        // var principal = await _accessTokenClient.GetClaimsPrincipalAsync(token, CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);

        // if (principal == null)
        // {
        //     throw new InvalidOperationException($"{nameof(AuthenticateAsync)}(..) received a null claims principal response.");
        // }
        //
        // await _authenticationStore.StoreAsync(httpContext, principal, token).ConfigureAwait(false);

        return AuthenticationResult.Success;
    }

    /// <summary>
    /// Determines if the client is a browser by checking the request for an accept header containing */* and a user-agent header containing Mozilla.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>True if the client appears to be a browser;  Otherwise false.</returns>
    internal static bool IsClientBrowser(HttpRequest? request)
    {
        if (request == null) return false;

        var isBrowser = request.Headers["Accept"].Any(a => a != null && a.Contains("*/*")) && request.Headers["User-Agent"].Any(a => a != null && a.Contains("Mozilla"));

        return isBrowser;
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        _optionsReloadToken?.Dispose();
    }
}
