using Azure.Core;
using Azure.Identity;
using GetUserClient.Configuration;

namespace GetUserClient.Authentication;

public class AzureAuthenticationServiceFactory
{
    private readonly IAuthenticationCacheManager _authenticationCacheManager;
    private readonly CliAuthenticationOptions? _authenticationOptions;
    
    public AzureAuthenticationServiceFactory(IAuthenticationCacheManager authenticationCacheManager, CliAuthenticationOptions? authOptions)
    {
        this._authenticationOptions = authOptions;
        this._authenticationCacheManager = authenticationCacheManager;
    }
    
    public virtual async Task<LoginServiceBase> GetAuthenticationServiceAsync(AuthenticationStrategy strategy, string? tenantId, string? clientId, string? certificateName, string? certificateThumbPrint, CancellationToken cancellationToken = default)
    {
        var credential = await GetTokenCredentialAsync(strategy, tenantId, clientId, certificateName, certificateThumbPrint, cancellationToken);
        return strategy switch
        {
            AuthenticationStrategy.DeviceCode when credential is DeviceCodeCredential deviceCred =>
                new InteractiveLoginService<DeviceCodeCredential>(deviceCred),
            AuthenticationStrategy.InteractiveBrowser when credential is InteractiveBrowserCredential browserCred =>
                new InteractiveLoginService<InteractiveBrowserCredential>(browserCred),
            _ => throw new InvalidOperationException($"The authentication strategy {strategy} is not supported")
        };
    }

    public virtual async Task<TokenCredential> GetTokenCredentialAsync(AuthenticationStrategy strategy, string? tenantId, string? clientId, string? certificateName, string? certificateThumbPrint, CancellationToken cancellationToken = default)
    {
        return strategy switch
        {
            AuthenticationStrategy.DeviceCode => await GetDeviceCodeCredentialAsync(tenantId, clientId,
                cancellationToken),
            AuthenticationStrategy.InteractiveBrowser => await GetInteractiveBrowserCredentialAsync(tenantId, clientId,
                cancellationToken),
            _ => throw new InvalidOperationException($"The authentication strategy {strategy} is not supported")
        };
    }
    
     private async Task<DeviceCodeCredential> GetDeviceCodeCredentialAsync(string? tenantId, string? clientId, CancellationToken cancellationToken = default)
    {
        DeviceCodeCredentialOptions credOptions = new()
        {
            ClientId = clientId ?? Constants.DefaultAppId,
            TenantId = tenantId ?? Constants.DefaultTenant,
            DisableAutomaticAuthentication = true,
        };

        TokenCachePersistenceOptions tokenCacheOptions = new() { Name = Constants.TokenCacheName };
        credOptions.TokenCachePersistenceOptions = tokenCacheOptions;
        credOptions.AuthenticationRecord = await _authenticationCacheManager.ReadAuthenticationRecordAsync(cancellationToken);

        return new DeviceCodeCredential(credOptions);
    }

    private async Task<InteractiveBrowserCredential> GetInteractiveBrowserCredentialAsync(string? tenantId, string? clientId, CancellationToken cancellationToken = default)
    {
        InteractiveBrowserCredentialOptions credOptions = new()
        {
            ClientId = clientId ?? Constants.DefaultAppId,
            TenantId = tenantId ?? Constants.DefaultTenant,
            DisableAutomaticAuthentication = true,
        };

        TokenCachePersistenceOptions tokenCacheOptions = new() { Name = Constants.TokenCacheName };
        credOptions.TokenCachePersistenceOptions = tokenCacheOptions;
        credOptions.AuthenticationRecord = await _authenticationCacheManager.ReadAuthenticationRecordAsync(cancellationToken);
        credOptions.LoginHint = credOptions.AuthenticationRecord?.Username;

        return new InteractiveBrowserCredential(credOptions);
    }
}