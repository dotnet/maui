using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.HybridWebApp;

sealed class MainPage : ContentPage
{
#if DEBUG
	readonly DevelopmentSession? _development;
#endif

	public MainPage()
	{
		var view = new HybridWebView
		{
			HybridRoot = "wwwroot",
			DefaultFile = "index.html"
		};
		Content = view;
#if DEBUG
		if (MauiProgram.Development is { } settings)
			_development = new DevelopmentSession(view, settings);
#endif
	}

#if DEBUG
	internal void StopDevelopment() => _development?.Dispose();
#endif
}
