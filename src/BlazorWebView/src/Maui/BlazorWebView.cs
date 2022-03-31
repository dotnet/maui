using System;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui.Controls;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// A <see cref="View"/> that can render Blazor content.
	/// </summary>
	public class BlazorWebView : View, IBlazorWebView
	{
		internal const string AppHostAddress = "0.0.0.0";

		private readonly JSComponentConfigurationStore _jSComponents = new();

		/// <summary>
		/// Initializes a new instance of <see cref="BlazorWebView"/>.
		/// </summary>
		public BlazorWebView()
		{
			RootComponents = new RootComponentsCollection(_jSComponents);
		}

		JSComponentConfigurationStore IBlazorWebView.JSComponents => _jSComponents;

		/// <summary>
		/// Gets or sets the path to the HTML file to render.
		/// <para>This is an app relative path to the file such as <c>wwwroot\index.html</c></para>
		/// </summary>
		public string? HostPage { get; set; }

		/// <inheritdoc />
		public RootComponentsCollection RootComponents { get; }

		/// <inheritdoc/>
		public event EventHandler<UrlLoadingEventArgs>? UrlLoading;

		/// <inheritdoc/>
		public virtual IFileProvider CreateFileProvider(string contentRootDir)
		{
			// Call into the platform-specific code to get that platform's asset file provider
			return ((BlazorWebViewHandler)(Handler!)).CreateFileProvider(contentRootDir);
		}

		internal void NotifyUrlLoading(UrlLoadingEventArgs args)
		{
			UrlLoading?.Invoke(this, args);
		}
	}
}
