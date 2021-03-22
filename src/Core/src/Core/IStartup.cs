using Microsoft.Maui.Hosting;

namespace Microsoft.Maui
{
	/// <summary>
	/// Startup allow to configure and instantiate application services.
	/// </summary>
	public interface IStartup
	{
		/// <summary>
		/// Configures the application.
		/// Configure are called by the .NET MAUI Core runtime when the app starts.
		/// </summary>
		/// <param name="appBuilder">Defines a class that provides the mechanisms to configure an application's dependencies.</param>
		void Configure(IAppHostBuilder appBuilder);
	}

	/// <summary>
	/// Allow to create a custom IAppHostBuilder instance.
	/// </summary>
	public interface IAppHostBuilderStartup : IStartup
	{
		/// <summary>
		/// Create and configure a builder object.
		/// </summary>
		/// <returns>The new instance of the IAppHostBuilder.</returns>
		IAppHostBuilder CreateAppHostBuilder();
	}
}