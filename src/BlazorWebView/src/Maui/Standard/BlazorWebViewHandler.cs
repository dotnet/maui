using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui.Handlers;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// A <see cref="ViewHandler"/> for <see cref="BlazorWebView"/>.
	/// </summary>
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, object>
	{
		/// <inheritdoc cref="Microsoft.Maui.Handlers.ViewHandler{TVirtualView, TPlatformView}.CreatePlatformView" />
		protected override object CreatePlatformView() => throw new NotSupportedException();

		/// <inheritdoc cref="IBlazorWebView.CreateFileProvider" />
		public virtual IFileProvider CreateFileProvider(string contentRootDir) => throw new NotSupportedException();
	}
}