using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
#if (SampleContent)
using MauiApp._1.Shared.Services;
using MauiApp._1.Web.Client.Services;
#endif

var builder = WebAssemblyHostBuilder.CreateDefault(args);

#if (SampleContent)
// Add device-specific services used by the MauiApp._1.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

#endif
#if (IndividualLocalAuth)
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

#endif
await builder.Build().RunAsync();
