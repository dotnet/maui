using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal class MauiBlazorWebViewBuilder : IMauiBlazorWebViewBuilder
	{
		public IServiceCollection Services { get; }

		public MauiBlazorWebViewBuilder(IServiceCollection services)
		{
			Services = services;
		}
	}
}
