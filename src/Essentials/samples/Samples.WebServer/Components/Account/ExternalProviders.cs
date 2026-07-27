using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Essentials.Samples.WebServer.Components.Account;

/// <summary>
/// Registers external OAuth sign-in providers from configuration. Each provider is added only when its
/// credentials are present (in appsettings.json or user-secrets), so the sample runs with none, some, or all
/// configured. <c>SaveTokens = true</c> keeps the provider's access token on the server. Add more providers
/// with any other <c>AuthenticationBuilder.AddX</c> handler.
/// </summary>
internal static class ExternalProviders
{
    public static AuthenticationBuilder AddConfiguredExternalProviders(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var auth = configuration.GetSection("Authentication");

        var google = auth.GetSection("Google");
        if (!string.IsNullOrEmpty(google["ClientId"]))
        {
            builder.AddGoogle(options =>
            {
                options.ClientId = google["ClientId"]!;
                options.ClientSecret = google["ClientSecret"]!;
                options.SaveTokens = true;
            });
        }

        var microsoft = auth.GetSection("Microsoft");
        if (!string.IsNullOrEmpty(microsoft["ClientId"]))
        {
            builder.AddMicrosoftAccount(options =>
            {
                options.ClientId = microsoft["ClientId"]!;
                options.ClientSecret = microsoft["ClientSecret"]!;
                options.SaveTokens = true;
            });
        }

        var facebook = auth.GetSection("Facebook");
        if (!string.IsNullOrEmpty(facebook["AppId"]))
        {
            builder.AddFacebook(options =>
            {
                options.AppId = facebook["AppId"]!;
                options.AppSecret = facebook["AppSecret"]!;
                options.SaveTokens = true;
            });
        }

        var apple = auth.GetSection("Apple");
        // Apple derives its client secret from the private key, so require the key — a partial Apple config
        // would otherwise fail options validation on every request and break the other providers too.
        if (!string.IsNullOrEmpty(apple["ClientId"]) && !string.IsNullOrEmpty(apple["PrivateKeyPath"]))
        {
            builder.AddApple(options =>
            {
                options.ClientId = apple["ClientId"]!;
                options.KeyId = apple["KeyId"]!;
                options.TeamId = apple["TeamId"]!;
                options.SaveTokens = true;
                options.UsePrivateKey(_ => environment.ContentRootFileProvider.GetFileInfo(apple["PrivateKeyPath"]!));
            });
        }

        return builder;
    }
}
