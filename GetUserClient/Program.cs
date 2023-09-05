using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.CommandLine.Hosting;
using Azure.Identity;
using GetUserClient;
using GetUserClient.ApiClient;
using GetUserClient.Authentication;
using GetUserClient.Commands;
using GetUserClient.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Authentication.Azure;
using Microsoft.Kiota.Cli.Commons.Extensions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Form;
using Microsoft.Kiota.Serialization.Json;
using Microsoft.Kiota.Serialization.Text;

var rootCommand = new GetUserApiClient().BuildRootCommand();
rootCommand.Description = "GetUserClient CLI";

//set up services
var builder = new CommandLineBuilder(rootCommand)
.UseDefaults()
.UseHost(HostBuilderFactory)
.UseRequestAdapter(ic =>
{
    var host = ic.GetHost();
    var serviceFactory = host.Services.GetRequiredService<AzureAuthenticationServiceFactory>();
    var credential = serviceFactory.GetTokenCredentialAsync(AuthenticationStrategy.DeviceCode, 
        "common", Constants.DefaultAppId, string.Empty, string.Empty);
    credential.Wait();

    var allowedHosts = new [] {"graph.microsoft.com"};
    var graphScopes = new [] {"User.Read"};
    // var options = new DeviceCodeCredentialOptions
    // {
    //     ClientId = "e8445ec0-848b-4148-a773-f6c4ae28ce36",
    //     DeviceCodeCallback = (code, _) => {
    //         Console.WriteLine(code.Message);
    //         return Task.FromResult(0);
    //     },
    // };
    //var credential = new DeviceCodeCredential(options);
    
    // Serializers needed for error parsing
    ApiClientBuilder.RegisterDefaultSerializer<JsonSerializationWriterFactory>();
    ApiClientBuilder.RegisterDefaultSerializer<TextSerializationWriterFactory>();
    ApiClientBuilder.RegisterDefaultSerializer<FormSerializationWriterFactory>();
    // Deserializers needed for error parsing & request body deserialization
    ApiClientBuilder.RegisterDefaultDeserializer<JsonParseNodeFactory>();
    ApiClientBuilder.RegisterDefaultDeserializer<TextParseNodeFactory>();
    ApiClientBuilder.RegisterDefaultDeserializer<FormParseNodeFactory>();
    
    var authProvider = new AzureIdentityAuthenticationProvider(credential.Result, allowedHosts, scopes: graphScopes);
    var adapter = new HttpClientRequestAdapter(authProvider);
    adapter.BaseUrl = "https://graph.microsoft.com/v1.0";
    return adapter;
}).RegisterCommonServices();

builder.AddMiddleware(async (ic, next) =>
{
    var host = ic.GetHost();
    ic.BindingContext.AddService(_ => host.Services.GetRequiredService<IAuthenticationCacheManager>());
    ic.BindingContext.AddService(_ => host.Services.GetRequiredService<AzureAuthenticationServiceFactory>());
    await next(ic);
});

rootCommand.AddCommand(new AzureLoginCommand(builder));

return await builder.Build().InvokeAsync(args);

static IHostBuilder HostBuilderFactory(string[] args) =>
    Host.CreateDefaultBuilder().ConfigureHostConfiguration((builder =>
    {
        builder.SetBasePath(Directory.GetCurrentDirectory());
    })).ConfigureAppConfiguration((context, builder) =>
        {
            var authCache = new AzureAuthenticationCacheManager();
            builder.AddJsonFile(authCache.GetAuthenticationCacheFilePath(), optional: true, reloadOnChange: true);
        })
        .ConfigureServices((context, collection) =>
    {
        collection.AddSingleton<IAuthenticationCacheManager, AzureAuthenticationCacheManager>();
        collection.AddSingleton<AzureAuthenticationServiceFactory>(provider =>
        {
            var options = new CliAuthenticationOptions()
            {
                ClientId = Constants.DefaultAppId,
                TenantId = "common"
            };
            var cacheUtil = provider.GetRequiredService<IAuthenticationCacheManager>();
            return new AzureAuthenticationServiceFactory(cacheUtil, options);
        });
    });

