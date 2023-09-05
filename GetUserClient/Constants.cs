using GetUserClient.Authentication;

namespace GetUserClient;

public class Constants
{
    public const string DefaultAppId = "e8445ec0-848b-4148-a773-f6c4ae28ce36";

    public const string DefaultTenant = "common";

    public const string DefaultAuthority = "https://login.microsoftonline.com";

    public const string TokenCacheName = "AzureCache";
    
    public const AuthenticationStrategy DefaultAuthStrategy = AuthenticationStrategy.DeviceCode;
    
    public const string AuthenticationIdCachePath = "authentication-id-cache.json";
}