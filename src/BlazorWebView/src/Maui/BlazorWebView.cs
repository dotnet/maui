using System;
using System.Collections.ObjectModel;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public class BlazorWebView : Microsoft.Maui.Controls.View, IBlazorWebView
	{
		public string? HostPage { get; set; }

		public ObservableCollection<RootComponent> RootComponents { get; } = new();
	}
}
