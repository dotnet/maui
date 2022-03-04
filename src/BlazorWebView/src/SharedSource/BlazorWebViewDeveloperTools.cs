using System;

#if WEBVIEW2_WINFORMS
namespace Microsoft.AspNetCore.Components.WebView.WindowsForms
#elif WEBVIEW2_WPF
namespace Microsoft.AspNetCore.Components.WebView.Wpf
#elif WEBVIEW2_MAUI
namespace Microsoft.AspNetCore.Components.WebView.Maui
#else
#error Must define WEBVIEW2_WINFORMS, WEBVIEW2_WPF, WEBVIEW2_MAUI
#endif
{
	internal class BlazorWebViewDeveloperTools
	{
		public bool Enabled { get; set; } = false;
	}
}
