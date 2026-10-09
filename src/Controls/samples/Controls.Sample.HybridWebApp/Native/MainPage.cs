using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.HybridWebApp;

sealed class MainPage : ContentPage
{
	public MainPage()
	{
		Content = new HybridWebView
		{
			HybridRoot = "wwwroot",
			DefaultFile = "index.html"
		};
	}
}
