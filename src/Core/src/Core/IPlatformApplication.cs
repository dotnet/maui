using System;

namespace Microsoft.Maui;

/// <summary>
/// Represents the platform-specific application instance that hosts a .NET MAUI application.
/// </summary>
/// <remarks>
/// This interface provides access to platform-specific services and the main application instance.
/// Each platform (Android, iOS, Windows, etc.) provides its own implementation of this interface.
/// Use the <c>IPlatformApplication.Current</c> property to access the current platform application instance.
/// </remarks>
public interface IPlatformApplication
{

#if !NETSTANDARD2_0
	/// <summary>
	/// Gets or sets the current platform application instance.
	/// </summary>
	/// <value>
	/// The current <see cref="IPlatformApplication"/> instance, or <see langword="null"/> if not set.
	/// </value>
	/// <remarks>
	/// <para>
	/// This property provides access to the platform-specific application instance and its services.
	/// It must be manually set by each platform implementation during application startup.
	/// </para>
	/// <para>
	/// Common usage scenarios:
	/// </para>
	/// <list type="bullet">
	/// <item><description>Accessing platform services: <c>IPlatformApplication.Current?.Services</c></description></item>
	/// <item><description>Getting the application instance: <c>IPlatformApplication.Current?.Application</c></description></item>
	/// <item><description>Platform-specific operations requiring the native application context</description></item>
	/// </list>
	/// <para>
	/// Always check for <see langword="null"/> before using this property, especially during application startup
	/// or in unit tests where the platform application may not be initialized.
	/// </para>
	/// </remarks>
	/// <example>
	/// <code>
	/// // Accessing a service from the platform application
	/// var platformApp = IPlatformApplication.Current;
	/// if (platformApp != null)
	/// {
	///     var myService = platformApp.Services.GetService&lt;IMyService&gt;();
	///     // Use the service...
	/// }
	/// </code>
	/// </example>
	public static IPlatformApplication? Current { get; set; }
#endif

	/// <summary>
	/// Gets the dependency injection service provider for the platform application.
	/// </summary>
	/// <value>
	/// An <see cref="IServiceProvider"/> instance containing platform-specific and application services.
	/// </value>
	/// <remarks>
	/// Use this service provider to resolve services that have been registered with the platform's
	/// dependency injection container. This includes both framework services and custom services
	/// registered during application configuration.
	/// </remarks>
	/// <example>
	/// <code>
	/// // Getting a service from the platform application
	/// var logger = platformApp.Services.GetService&lt;ILogger&gt;();
	/// var httpClient = platformApp.Services.GetRequiredService&lt;HttpClient&gt;();
	/// </code>
	/// </example>
	public IServiceProvider Services { get; }

	/// <summary>
	/// Gets the .NET MAUI application instance.
	/// </summary>
	/// <value>
	/// An <see cref="IApplication"/> instance representing the current MAUI application.
	/// </value>
	/// <remarks>
	/// This property provides access to the main MAUI application instance, which contains
	/// application-level configuration, the main page, and application lifecycle methods.
	/// Use this to access application-wide properties and methods.
	/// </remarks>
	/// <example>
	/// <code>
	/// // Accessing the main page from the application
	/// var mainPage = platformApp.Application.MainPage;
	/// 
	/// // Triggering application lifecycle events
	/// if (platformApp.Application is Application app)
	/// {
	///     app.OnStart();
	/// }
	/// </code>
	/// </example>
	public IApplication Application { get; }
}
