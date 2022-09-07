using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui.Handlers;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// A <see cref="ViewHandler"/> for <see cref="BlazorWebView"/>.
	/// </summary>
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, Gtk.Widget>
	{
		/// <inheritdoc />
		protected override Gtk.Widget CreatePlatformView() => throw new NotSupportedException();

		/// <inheritdoc />
		public virtual IFileProvider CreateFileProvider(string contentRootDir) => throw new NotSupportedException();

		private void StartWebViewCoreIfPossible() { }
		
#pragma warning disable CS0649
		private WebViewManager? _webviewManager;
#pragma warning restore CS0649

	}
}