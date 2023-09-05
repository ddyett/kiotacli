using Azure.Identity;
using GetUserClient.Configuration;

namespace GetUserClient.Authentication;

public interface IAuthenticationCacheManager
{
    string GetAuthenticationCacheFilePath();

    Task SaveAuthenticationIdentifiersAsync(CliAuthenticationOptions authenticationOptions, CancellationToken cancellationToken = default);

    Task<CliAuthenticationOptions> ReadAuthenticationIdentifiersAsync(CancellationToken cancellationToken = default);

    Task<AuthenticationRecord?> ReadAuthenticationRecordAsync(CancellationToken cancellationToken = default);

    Task ClearTokenCache(CancellationToken cancellationToken = default);
}