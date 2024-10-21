using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting
{
	/// <summary>
	/// Extension methods to <see cref="IServiceCollection"/> for use with the HybridWebView.
	/// </summary>
	public static class HybridWebViewServiceCollectionExtensions
	{
		/// <summary>
		/// Enables browser Developer tools on the underlying WebView controls used by the HybridWebView.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/>.</param>
		/// <returns>The <see cref="IServiceCollection"/>.</returns>
		public static IServiceCollection AddHybridWebViewDeveloperTools(this IServiceCollection services)
		{
			return services.AddSingleton<HybridWebViewDeveloperTools>(new HybridWebViewDeveloperTools { Enabled = true });
		}
	}
}
