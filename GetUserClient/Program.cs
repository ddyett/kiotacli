using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.CommandLine.Hosting;
using System.CommandLine.IO;
using Azure.Identity;
using GetUserClient;
using GetUserClient.ApiClient;
using GetUserClient.ApiClient.Models.ODataErrors;
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
    //var graphScopes = new [] {"User.Read", "Calendar.Read"};
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
    
    var authProvider = new AzureIdentityAuthenticationProvider(credential.Result, allowedHosts);
    var adapter = new HttpClientRequestAdapter(authProvider);
    adapter.BaseUrl = "https://graph.microsoft.com/v1.0";
    return adapter;
}).RegisterCommonServices();
builder.UseExceptionHandler((ex, context) =>
{
    var message = ex switch
    {
        _ when ex is AuthenticationRequiredException =>
            "Token acquisition failed. Run mgc login command first to get an access token.",
        _ when ex is TaskCanceledException => string.Empty,
        ODataError _e when ex is ODataError =>
            $"Error {_e.ResponseStatusCode}({_e.Error?.Code}) from API:\n  {_e.Error?.Message}",
        ApiException _e when ex is ApiException => $"Error {_e.ResponseStatusCode} from API.",
        _ => ex.Message
    };

    var exitCode = ex switch
    {
        _ when ex is AuthenticationRequiredException => 1,
        _ when ex is TaskCanceledException => 0,
        _ => -1
    };

    if (!string.IsNullOrEmpty(message))
    {
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Red;
        context.Console.Error.WriteLine(message);
        Console.ResetColor();
    }

    context.ExitCode = exitCode;
});

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
                TenantId = "common",
            };
            var cacheUtil = provider.GetRequiredService<IAuthenticationCacheManager>();
            return new AzureAuthenticationServiceFactory(cacheUtil, options);
        });
    });

