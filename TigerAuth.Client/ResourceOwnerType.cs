namespace TigerAuth.Client;

public enum ResourceOwnerType
{
    None = 0,
    Teller = 1,
    Customer = 2,
    DomainUser = 3,
    Trusted = 4,
    AzureAdSaml = 5,
}