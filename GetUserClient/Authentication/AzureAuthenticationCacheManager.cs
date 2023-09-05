using Azure.Identity;
using GetUserClient.Configuration;
using System.Text.Json;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace GetUserClient.Authentication;

public class AzureAuthenticationCacheManager : IAuthenticationCacheManager
{
    public string GetAuthenticationCacheFilePath()
    {
        var filePath = ApplicationFilePath;
        return Path.Join(filePath, Constants.AuthenticationIdCachePath);
    }

    private static string ApplicationFilePath
    {
        get
        {
            var filePath = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? AppContext.BaseDirectory, ".mgc");
            Directory.CreateDirectory(filePath);
            return filePath;
        }
    }

    private async Task<ConfigurationRoot?> ReadConfigurationAsync(
        CancellationToken cancellationToken = default (CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        var authenticationCacheFilePath = GetAuthenticationCacheFilePath();
        if (!File.Exists(authenticationCacheFilePath))
            return null;
        await using FileStream fileStream = File.OpenRead(authenticationCacheFilePath);
        try
        {
            return await JsonSerializer.DeserializeAsync<ConfigurationRoot>(fileStream, cancellationToken: cancellationToken);
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    private async Task WriteConfigurationAsync(
        string path,
        ConfigurationRoot configuration,
        CancellationToken cancellationToken = default (CancellationToken),
        int retryCount = 0)
    {
        try
        {
            await using FileStream fileStream = File.Open(path, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(fileStream, configuration, cancellationToken: cancellationToken);
        }
        catch (DirectoryNotFoundException)
        {
            Directory.CreateDirectory(path);
            if (retryCount >= 1)
                return;
            await this.WriteConfigurationAsync(path, configuration, cancellationToken, retryCount + 1);
        }
    }

    public async Task SaveAuthenticationIdentifiersAsync(CliAuthenticationOptions authenticationOptions, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = this.GetAuthenticationCacheFilePath();
        ConfigurationRoot configuration = await this.ReadConfigurationAsync(cancellationToken) ?? new ConfigurationRoot();
        CliAuthenticationOptions configurationAuthenticationOptions = configuration.AuthenticationOptions;
        const string authenticationHost = "https://login.microsoftonline.com";
        if (configurationAuthenticationOptions.ClientId == authenticationOptions.ClientId && configurationAuthenticationOptions.TenantId == authenticationOptions.TenantId && 
            configurationAuthenticationOptions.ClientCertificateName == authenticationOptions.ClientCertificateName && 
            configurationAuthenticationOptions.ClientCertificateThumbPrint == authenticationOptions.ClientCertificateThumbPrint && 
            configurationAuthenticationOptions.Strategy == authenticationOptions.Strategy && authenticationHost == configurationAuthenticationOptions.Authority)
        {
        }
        else
        {
            configuration.AuthenticationOptions = new CliAuthenticationOptions()
            {
                Authority = authenticationHost,
                ClientId = authenticationOptions.ClientId,
                TenantId = authenticationOptions.TenantId,
                ClientCertificateName = authenticationOptions.ClientCertificateName,
                ClientCertificateThumbPrint = authenticationOptions.ClientCertificateThumbPrint,
                Strategy = authenticationOptions.Strategy
            };
            
            await this.WriteConfigurationAsync(path, configuration, cancellationToken);
        }
    }

    public async Task<CliAuthenticationOptions> ReadAuthenticationIdentifiersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!File.Exists(this.GetAuthenticationCacheFilePath()))
            throw new FileNotFoundException();
        ConfigurationRoot? configurationRoot = await this.ReadConfigurationAsync(cancellationToken);
        return configurationRoot?.AuthenticationOptions != null ? configurationRoot.AuthenticationOptions : throw new Exception("Cannot find cached authentication identifiers.");
    }

    public async Task<AuthenticationRecord?> ReadAuthenticationRecordAsync(CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(ApplicationFilePath, "authRecord");
        var authenticationRecord = (AuthenticationRecord) null!;
        if (!File.Exists(path)) return authenticationRecord;
        
        await using var authRecordStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        authenticationRecord = await AuthenticationRecord.DeserializeAsync(authRecordStream, cancellationToken);
        return authenticationRecord;
    }

    public async Task ClearTokenCache(CancellationToken cancellationToken = default)
    {
        CliAuthenticationOptions options = await this.ReadAuthenticationIdentifiersAsync(cancellationToken);
        AuthenticationRecord? authenticationRecord = await this.ReadAuthenticationRecordAsync(cancellationToken);
        var clientId = authenticationRecord?.ClientId ?? options.ClientId;

        IClientApplicationBase? app;

        if (!options.Strategy.IsPrivateClient())
        {
            MsalCacheHelper cacheHelper = await this.GetProtectedCacheHelperAsync("MicrosoftGraph");
            app = PublicClientApplicationBuilder.Create(clientId).Build();
            cacheHelper.RegisterCache(app.UserTokenCache);
            using IEnumerator<IAccount> accountsIter = (await app.GetAccountsAsync()).GetEnumerator();
            while (accountsIter.MoveNext())
                await app.RemoveAsync(accountsIter.Current);
            cacheHelper.UnregisterCache(app.UserTokenCache);
        }

        this.DeleteAuthenticationIdentifiers();
        this.DeleteAuthenticationRecord();
    }

    private void DeleteAuthenticationIdentifiers()
    {
        string authenticationCacheFilePath = this.GetAuthenticationCacheFilePath();
        if (!File.Exists(authenticationCacheFilePath))
            return;
        File.Delete(authenticationCacheFilePath);
    }

    private void DeleteAuthenticationRecord()
    {
        string path = Path.Combine(ApplicationFilePath, "authRecord");
        if (!File.Exists(path))
            return;
        File.Delete(path);
    }
    private async Task<MsalCacheHelper> GetProtectedCacheHelperAsync(
        string name)
    {
        var cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".IdentityService");
        var attribute1 = new KeyValuePair<string, string>("MsalClientID", "Microsoft.Developer.IdentityService");
        var attribute2 = new KeyValuePair<string, string>("Microsoft.Developer.IdentityService", "1.0.0.0");
        return await MsalCacheHelper.CreateAsync(new StorageCreationPropertiesBuilder(name, cacheDirectory).WithMacKeyChain("Microsoft.Developer.IdentityService", name).WithLinuxKeyring("msal.cache", "default", name, attribute1, attribute2).Build());
    }
}