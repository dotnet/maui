using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Embedding;

/// <summary>
/// A set of extension methods that allow for embedding a MAUI view within a native application.
/// </summary>
internal static class EmbeddingExtensions
{
	/// <summary>
	/// Configures the <see cref="MauiAppBuilder"/> to use the specified <typeparamref name="TApp"/> as the embedded application type.
	/// </summary>
	/// <typeparam name="TApp">The type to use as the embedded application.</typeparam>
	/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
	/// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
	public static MauiAppBuilder UseMauiEmbeddedApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder)
		where TApp : class, IApplication
	{
		builder.UseMauiApp<TApp>();
#if ANDROID || IOS || MACCATALYST || WINDOWS
		builder.UseMauiEmbedding();
#endif
		return builder;
	}

	/// <summary>
	/// Configures the <see cref="MauiAppBuilder"/> to use the specified <typeparamref name="TApp"/> as the embedded application type.
	/// </summary>
	/// <typeparam name="TApp">The type to use as the embedded application.</typeparam>
	/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
	/// <param name="implementationFactory">A factory to create the specified <typeparamref name="TApp"/> using the services provided in a <see cref="IServiceProvider"/>.</param>
	/// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
	public static MauiAppBuilder UseMauiEmbeddedApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder, Func<IServiceProvider, TApp> implementationFactory)
		where TApp : class, IApplication
	{
		builder.UseMauiApp<TApp>(implementationFactory);
#if ANDROID || IOS || MACCATALYST || WINDOWS
		builder.UseMauiEmbedding();
#endif
		return builder;
	}
}
