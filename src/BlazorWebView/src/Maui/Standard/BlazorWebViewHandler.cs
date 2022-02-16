using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui.Handlers;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public virtual IFileProvider CreateFileProvider(string contentRootDir) => throw new NotImplementedException();
	}
}