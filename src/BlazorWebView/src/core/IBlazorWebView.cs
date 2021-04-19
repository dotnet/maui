using Microsoft.Maui;
using System;
using System.Collections.ObjectModel;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public interface IBlazorWebView : IView
	{
		string? HostPage { get; set; }
		ObservableCollection<RootComponent> RootComponents { get; }
		IServiceProvider? Services { get; set; }
	}
}
