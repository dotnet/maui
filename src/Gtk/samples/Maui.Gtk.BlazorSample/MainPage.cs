using Maui.Gtk.BlazorSample.Components;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Gtk.BlazorSample;

/// <summary>A page that is one BlazorWebView; wwwroot/index.html is the host page.</summary>
public sealed class MainPage : ContentPage
{
	public MainPage()
	{
		var blazor = new BlazorWebView { HostPage = "wwwroot/index.html" };
		blazor.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(Counter) });
		Content = blazor;
	}
}
