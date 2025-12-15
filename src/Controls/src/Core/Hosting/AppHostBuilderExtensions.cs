using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls.Diagnostics;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;

#if ANDROID
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#elif WINDOWS
using ResourcesProvider = Microsoft.Maui.Controls.Compatibility.Platform.UWP.WindowsResourcesProvider;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
#elif IOS || MACCATALYST
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Handlers.Items2;
#elif TIZEN
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
#endif

namespace Microsoft.Maui.Controls.Hosting;

public static partial class AppHostBuilderExtensions
{
	/// <summary>
	/// Configures the <see cref="MauiAppBuilder"/> to use the specified <typeparamref name="TApp"/> as the main application type.
	/// </summary>
	/// <typeparam name="TApp">The type to use as the application.</typeparam>
	/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
	/// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
	internal static MauiAppBuilder UseMauiPrimaryApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder)
		where TApp : class, IApplication
	{
#pragma warning disable RS0030 // Do not used banned APIs - don't want to use a factory method here
		builder.Services.TryAddSingleton<IApplication, TApp>();
#pragma warning restore RS0030
		builder.SetupDefaults();
		return builder;
	}

	/// <summary>
	/// Configures the <see cref="MauiAppBuilder"/> to use the specified <typeparamref name="TApp"/> as the main application type.
	/// </summary>
	/// <typeparam name="TApp">The type to use as the application.</typeparam>
	/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
	/// <param name="implementationFactory">A factory to create the specified <typeparamref name="TApp"/> using the services provided in a <see cref="IServiceProvider"/>.</param>
	/// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
	internal static MauiAppBuilder UseMauiPrimaryApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder, Func<IServiceProvider, TApp> implementationFactory)
		where TApp : class, IApplication
	{
		builder.Services.TryAddSingleton<IApplication>(implementationFactory);
		builder.SetupDefaults();
		return builder;
	}

	static MauiAppBuilder SetupDefaults(this MauiAppBuilder builder)
	{
#if WINDOWS || ANDROID || IOS || MACCATALYST || TIZEN
		// initialize compatibility DependencyService
		DependencyService.SetToInitialized();

#pragma warning disable CS0612, CA1416 // Type or member is obsolete, 'ResourcesProvider' is unsupported on: 'iOS' 14.0 and later
		DependencyService.Register<ResourcesProvider>();
		DependencyService.Register<FontNamedSizeService>();
#pragma warning restore CS0612, CA1416 // Type or member is obsolete
#endif
		builder.Services.AddScoped(_ => new HideSoftInputOnTappedChangedManager());

		builder.ConfigureImageSourceHandlers();

		builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddControlsHandlers();
		});

		// NOTE: not registered under NativeAOT or TrimMode=Full scenarios
		if (RuntimeFeature.IsHybridWebViewSupported)
		{
			builder.Services.AddScoped<IHybridWebViewTaskManager>(_ => new HybridWebViewTaskManager());
		}

		builder.ConfigureMauiControlsDiagnostics();

#if WINDOWS
		builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, MauiControlsInitializer>());
#endif

		return builder;
	}

	class MauiControlsInitializer : IMauiInitializeService
	{
		public void Initialize(IServiceProvider services)
		{
#if WINDOWS
			var dispatcher = services.GetRequiredApplicationDispatcher();

			dispatcher
				.DispatchIfRequired(() =>
				{
					var dictionaries = UI.Xaml.Application.Current?.Resources?.MergedDictionaries;
					if (dictionaries != null)
					{
						// Microsoft.Maui.Controls
						UI.Xaml.Application.Current?.Resources?.AddLibraryResources("MicrosoftMauiControlsIncluded", "ms-appx:///Microsoft.Maui.Controls/Platform/Windows/Styles/Resources.xbf");
					}
				});
#endif
		}
	}

	internal static IMauiHandlersCollection AddControlsHandlers(this IMauiHandlersCollection handlersCollection)
	{
		handlersCollection.AddHandler<IContentView, ContentViewHandler>();

		return handlersCollection;
	}

	static MauiAppBuilder ConfigureImageSourceHandlers(this MauiAppBuilder builder)
	{
		builder.ConfigureImageSources(services =>
		{
			services.AddService<FileImageSource>(svcs => new FileImageSourceService(svcs.CreateLogger<FileImageSourceService>()));
			services.AddService<FontImageSource>(svcs => new FontImageSourceService(svcs.GetRequiredService<IFontManager>(), svcs.CreateLogger<FontImageSourceService>()));
			services.AddService<StreamImageSource>(svcs => new StreamImageSourceService(svcs.CreateLogger<StreamImageSourceService>()));
			services.AddService<UriImageSource>(svcs => new UriImageSourceService(svcs.CreateLogger<UriImageSourceService>()));
		});

		return builder;
	}
}
