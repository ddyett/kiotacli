using System.CommandLine;
using System.CommandLine.Builder;
using System.Text;
using GetUserClient.Authentication;
using GetUserClient.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GetUserClient.Commands;

public sealed class AzureLoginCommand:System.CommandLine.Command
{
    private readonly Option<string> _clientIdOption = new("--client-id", "The client (application) id");

    private readonly Option<string> _tenantIdOption = new("--tenant-id", "The tenant (directory) id");

    private readonly Option<string> _certificateNameOption = new("--certificate-name", "The name of your certificate (e.g. CN=MyCertificate). The certificate will be retrieved from the current user's certificate store.");

    private readonly Option<string> _certificateThumbPrintOption = new("--certificate-thumb-print", "The thumbprint of your certificate. The certificate will be retrieved from the current user's certificate store.");

    private readonly Option<AuthenticationStrategy> _strategyOption = new("--strategy", () => AuthenticationStrategy.DeviceCode);
    
    private readonly Option<string[]> _scopesOption = new("--scopes", 
        "The login scopes e.g. User.Read. Required scopes can be found in the docs linked against each verb (get, list, create...) command.")
    {
        Arity = ArgumentArity.ZeroOrMore,
        AllowMultipleArgumentsPerToken = true,
        IsRequired = false
    };
    
    internal AzureLoginCommand() : base("login", "Special login command")
    {
        AddOption(_scopesOption);
        AddOption(_clientIdOption);
        AddOption(_tenantIdOption);
        AddOption(_certificateNameOption);
        AddOption(_certificateThumbPrintOption);
        AddOption(_strategyOption);
        
        this.SetHandler(async (context) =>
        {
            var scopes = (context.ParseResult.GetValueForOption(_scopesOption) ?? new string[] { }) as string[];
            var clientId = context.ParseResult.GetValueForOption(_clientIdOption) ?? Constants.DefaultAppId;
            var tenantId = context.ParseResult.GetValueForOption(_tenantIdOption) ?? Constants.DefaultTenant;
            var certificateName = context.ParseResult.GetValueForOption(_certificateNameOption);
            var certificateThumbPrint = context.ParseResult.GetValueForOption(_certificateThumbPrintOption);
            var strategy = context.ParseResult.GetValueForOption(_strategyOption);
            var cancellationToken = context.GetCancellationToken();

            var authUtil = context.BindingContext.GetRequiredService<IAuthenticationCacheManager>();
            var authSvcFactory = context.BindingContext.GetRequiredService<AzureAuthenticationServiceFactory>();

            var authService = await authSvcFactory.GetAuthenticationServiceAsync(strategy, tenantId, clientId, certificateName, certificateThumbPrint, cancellationToken);
            await authService.LoginAsync(scopes, cancellationToken);
            var cliOptions = new CliAuthenticationOptions()
            {
                ClientId = clientId,
                TenantId = tenantId,
                ClientCertificateName = certificateName,
                ClientCertificateThumbPrint = certificateThumbPrint,
                Strategy = strategy
            };
            
            await authUtil.SaveAuthenticationIdentifiersAsync(cliOptions, cancellationToken);
        });
    }
    
    public AzureLoginCommand(CommandLineBuilder builder) : this()
    {
        builder?.UseHelp((ctx) =>
        {
            ctx.HelpBuilder.CustomizeSymbol(_strategyOption, firstColumnText: (ctx) => "--strategy <strategy>", secondColumnText: (ctx) =>
            {
                var builder = new StringBuilder($"The authentication strategy to use. [default: {Constants.DefaultAuthStrategy}]\n\n  Available strateges:\n    ");
                builder.Append(nameof(AuthenticationStrategy.DeviceCode));
                builder.Append(":         Use a device code to log in.\n    ");
                builder.Append(nameof(AuthenticationStrategy.InteractiveBrowser));
                builder.Append(": Open a browser on this computer to log in.\n    ");
                return builder.ToString();
            });
        });
    }
}