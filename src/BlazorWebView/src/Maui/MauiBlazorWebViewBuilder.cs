using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal class MauiBlazorWebViewBuilder : IMauiBlazorWebViewBuilder
	{
		/// <inheritdoc />
		public IServiceCollection Services { get; }

		internal MauiBlazorWebViewBuilder(IServiceCollection services)
		{
			Services = services;
		}
	}
}
