#if (IndividualLocalAuth)
#if (UseServer)
using Microsoft.AspNetCore.Components.Authorization;
#endif
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
#endif
#if (UseWebAssembly && SampleContent)
#endif
using MauiApp._1.Web.Components;
#if (IndividualLocalAuth)
using MauiApp._1.Web.Components.Account;
using MauiApp._1.Web.Data;
#endif
#if (SampleContent)
using MauiApp._1.Shared.Services;
using MauiApp._1.Web.Services;
#endif

namespace MauiApp._1;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        #if (!UseServer && !UseWebAssembly)
        builder.Services.AddRazorComponents();
        #else
        builder.Services.AddRazorComponents()
          #if (UseServer && UseWebAssembly && IndividualLocalAuth)
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization();
          #elif (UseServer && UseWebAssembly)
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
          #elif (UseServer)
            .AddInteractiveServerComponents();
          #elif (UseWebAssembly && IndividualLocalAuth)
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization();
          #elif (UseWebAssembly)
            .AddInteractiveWebAssemblyComponents();
          #endif
        #endif

        #if (SampleContent)
        // Add device-specific services used by the MauiApp._1.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();

        #endif
        #if (IndividualLocalAuth)
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityRedirectManager>();
        #if (UseServer)
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
        #endif

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();
        #if (!UseServer)
        builder.Services.AddAuthorization();
        #endif

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        #if (UseLocalDB)
            options.UseSqlServer(connectionString));
        #else
            options.UseSqlite(connectionString));
        #endif
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

        #endif
        var app = builder.Build();

        // Configure the HTTP request pipeline.
#if (UseWebAssembly || IndividualLocalAuth)
        if (app.Environment.IsDevelopment())
        {
#if (UseWebAssembly)
            app.UseWebAssemblyDebugging();
#endif
#if (IndividualLocalAuth)
            app.UseMigrationsEndPoint();
#endif
        }
        else
#else
        if (!app.Environment.IsDevelopment())
#endif
        {
            app.UseExceptionHandler("/Error");
        #if (HasHttpsProfile)
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        #endif
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        #if (HasHttpsProfile)
        app.UseHttpsRedirection();

        #endif
        app.UseAntiforgery();

        app.MapStaticAssets();
        #if (UseServer && UseWebAssembly)
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(
                typeof(MauiApp._1.Shared._Imports).Assembly,
                typeof(MauiApp._1.Web.Client._Imports).Assembly);
        #elif (UseServer)
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(
                typeof(MauiApp._1.Shared._Imports).Assembly);
        #elif (UseWebAssembly)
        app.MapRazorComponents<App>()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(
                typeof(MauiApp._1.Shared._Imports).Assembly,
                typeof(MauiApp._1.Web.Client._Imports).Assembly);
        #else
        app.MapRazorComponents<App>()
            .AddAdditionalAssemblies(
                typeof(MauiApp._1.Shared._Imports).Assembly);
        #endif

        #if (IndividualLocalAuth)
        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        #endif
        app.Run();
    }
}
