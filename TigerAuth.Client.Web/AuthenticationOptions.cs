namespace TigerAuth.Client.Web;

public class AuthenticationOptions
{
    public int? ApiVersion { get; set; }

    public string? ApplicationName { get; set; }

    public string? ApplicationSecret { get; set; }

    public string? AuthorizationServiceUrl { get; set; }

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string? EnvironmentName { get; set; }

    public bool IsLiveData { get; set; }

    public ResourceOwnerType? ResourceOwnerType { get; set; }

    public string? Scope { get; set; }

    public string? ApplicationVersion { get; set; }
}
