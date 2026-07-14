using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Hosting;

public static partial class AppHostBuilderExtensions
{
	/// <summary>
	/// Configures the <see cref="MauiAppBuilder"/> to use the specified <typeparamref name="TApp"/> as the main application type.
	/// </summary>
	/// <typeparam name="TApp">The type to use as the application.</typeparam>
	/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
	/// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
	public static MauiAppBuilder UseMauiApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder)
		where TApp : class, IApplication
	{
		builder.UseMauiPrimaryApp<TApp>();
		builder.SetupXamlDefaults();
		return builder;
	}

	/// <summary>
	/// Configures the <see cref="MauiAppBuilder"/> to use the specified <typeparamref name="TApp"/> as the main application type.
	/// </summary>
	/// <typeparam name="TApp">The type to use as the application.</typeparam>
	/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
	/// <param name="implementationFactory">A factory to create the specified <typeparamref name="TApp"/> using the services provided in a <see cref="IServiceProvider"/>.</param>
	/// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
	public static MauiAppBuilder UseMauiApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder, Func<IServiceProvider, TApp> implementationFactory)
		where TApp : class, IApplication
	{
		builder.UseMauiPrimaryApp<TApp>(implementationFactory);
		builder.SetupXamlDefaults();
		return builder;
	}

	/// <summary>
	/// Returns the handlers collection unchanged. Built-in .NET MAUI Controls handlers are resolved automatically.
	/// </summary>
	/// <param name="handlersCollection">The handlers collection to return.</param>
	/// <returns>The handlers collection for chaining.</returns>
	[Obsolete("AddMauiControlsHandlers is no longer required and is a no-op. Built-in .NET MAUI Controls handlers are now resolved automatically via [ElementHandler] attributes on the view types, so this call can be removed. To override a built-in handler or register one that requires DI, use ConfigureMauiHandlers(handlers => handlers.AddHandler<TView, THandler>()) instead.")]
	public static IMauiHandlersCollection AddMauiControlsHandlers(this IMauiHandlersCollection handlersCollection) =>
		handlersCollection;

	static MauiAppBuilder SetupXamlDefaults(this MauiAppBuilder builder)
	{
#if WINDOWS || ANDROID || IOS || MACCATALYST || TIZEN
		DependencyService.Register<Xaml.ValueConverterProvider>();
#endif
		return builder;
	}
}
