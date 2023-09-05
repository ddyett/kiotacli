using Azure.Identity;

namespace GetUserClient.Authentication;

public abstract class LoginServiceBase
{
    public async Task LoginAsync(string[] scopes, CancellationToken cancellationToken = default)
    {
        var record = await this.DoLoginAsync(scopes, cancellationToken);
        await this.SaveSessionAsync(record, cancellationToken);
    }
    
    protected abstract Task<AuthenticationRecord?> DoLoginAsync(string[] scopes, CancellationToken cancellationToken = default);

    public async Task SaveSessionAsync(AuthenticationRecord? record = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (record is null) return;
        var recordPath = Path.Combine(Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? AppContext.BaseDirectory,".mgc"), "authRecord");
        await using var authRecordStream = new FileStream(recordPath, FileMode.Create, FileAccess.Write);
        await record.SerializeAsync(authRecordStream, cancellationToken);
    }
}