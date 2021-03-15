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
}