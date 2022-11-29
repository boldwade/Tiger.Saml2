namespace TigerAuth.Client.Web;

public class AuthenticationResult
{
    public static AuthenticationResult Success { get; } = new(null);

    public Uri? Redirect { get; }

    public AuthenticationResult(Uri? redirect)
    {
        Redirect = redirect;
    }
}
