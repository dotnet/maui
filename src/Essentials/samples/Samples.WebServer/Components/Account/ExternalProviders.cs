using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Essentials.Samples.WebServer.Components.Account;

/// <summary>
/// Registers external OAuth sign-in providers from configuration. Each provider is only added when its
/// credentials are present (in appsettings.json or user-secrets), so the sample works with none, some, or
/// all of them configured. These are the real ASP.NET Core provider handlers — there is no mock: whatever
/// is registered here is what the native app discovers and can sign in with.
///
/// <c>SaveTokens = true</c> keeps the provider's access token on the SERVER so the app never handles it
/// (Backend-for-Frontend). Add more providers the same way with any other <c>AuthenticationBuilder.AddX</c>
/// handler — the native flow and discovery are provider-agnostic.
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
        if (!string.IsNullOrEmpty(apple["ClientId"]))
        {
            builder.AddApple(options =>
            {
                options.ClientId = apple["ClientId"]!;
                options.KeyId = apple["KeyId"]!;
                options.TeamId = apple["TeamId"]!;
                options.SaveTokens = true;

                var privateKeyPath = apple["PrivateKeyPath"];
                if (!string.IsNullOrEmpty(privateKeyPath))
                    options.UsePrivateKey(_ => environment.ContentRootFileProvider.GetFileInfo(privateKeyPath));
            });
        }

        return builder;
    }
}
