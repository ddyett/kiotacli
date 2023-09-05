using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;

namespace GetUserClient.Authentication;

public class InteractiveLoginService<T> : LoginServiceBase where T:TokenCredential
{
    private T _credential;
    
    public InteractiveLoginService(T credential) : base()
    {
        if (credential is not DeviceCodeCredential && credential is not InteractiveBrowserCredential)
        {
            throw new ArgumentException($"The provided credential {credential.GetType().Name} does not support interactive login." +
                                        $"Supported types are:\n {nameof(DeviceCodeCredential)}\n {nameof(InteractiveBrowserCredential)}", nameof(credential));
        }

        this._credential = credential;
    }

    protected override async Task<AuthenticationRecord?> DoLoginAsync(string[] scopes, CancellationToken cancellationToken = default)
    {
        if (_credential is DeviceCodeCredential deviceCodeCred)
        {
            return await deviceCodeCred.AuthenticateAsync(new TokenRequestContext(scopes), cancellationToken);
        }
        else if (_credential is InteractiveBrowserCredential browserCred)
        {
            return await browserCred.AuthenticateAsync(new TokenRequestContext(scopes), cancellationToken);
        }

        // Due to the check in the constructor, this code shouldn't be reachable normally.
        throw new InvalidOperationException("The provided credential is not supported.");
    }
}