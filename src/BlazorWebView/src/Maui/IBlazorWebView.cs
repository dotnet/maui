using System;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public interface IBlazorWebView : IView
	{
		string? HostPage { get; set; }
		RootComponentsCollection RootComponents { get; }
		JSComponentConfigurationStore JSComponents { get; }

		/// <summary>
		/// Allows customizing how external links are opened.
		/// Opens external links in the system browser by default.
		/// </summary>
		event EventHandler<ExternalLinkNavigationInfo>? OnExternalNavigationStarting;

		/// <summary>
		/// Creates a file provider for static assets used in the <see cref="BlazorWebView"/>. The default implementation
		/// serves files from a platform-specific location. Override this method to return a custom <see cref="IFileProvider"/> to serve assets such
		/// as <c>wwwroot/index.html</c>. Call the base method and combine its return value with a <see cref="CompositeFileProvider"/>
		/// to use both custom assets and default assets.
		/// </summary>
		/// <param name="contentRootDir">The base directory to use for all requested assets, such as <c>wwwroot</c>.</param>
		/// <returns>Returns a <see cref="IFileProvider"/> for static assets.</returns>
		public IFileProvider CreateFileProvider(string contentRootDir);
	}
}
