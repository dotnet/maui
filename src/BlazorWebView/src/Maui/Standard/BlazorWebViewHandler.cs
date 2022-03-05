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
		/// <inheritdoc />
		protected override object CreatePlatformView() => throw new NotSupportedException();

		/// <inheritdoc />
		public virtual IFileProvider CreateFileProvider(string contentRootDir) => throw new NotSupportedException();
	}
}