using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();
	}
}