using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// A builder for .NET MAUI Blazor WebViews.
	/// </summary>
	public interface IMauiBlazorWebViewBuilder
	{
		/// <summary>
		/// Gets the builder service collection.
		/// </summary>
		IServiceCollection Services { get; }
	}
}
