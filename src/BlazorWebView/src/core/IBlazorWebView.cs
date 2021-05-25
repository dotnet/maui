using System;
using System.Collections.ObjectModel;
using Microsoft.Maui;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public interface IBlazorWebView : IView
	{
		string? HostPage { get; set; }
		ObservableCollection<RootComponent> RootComponents { get; }
	}
}
