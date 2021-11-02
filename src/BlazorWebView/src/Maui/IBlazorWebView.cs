using Microsoft.AspNetCore.Components.Web;
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
		/// Creates a file provider for static assets used in the <see cref="BlazorWebView"/>. Override
		/// this method to return a custom <see cref="IFileProvider"/> to serve assets such as <c>wwwroot/index.html</c>.
		/// </summary>
		/// <param name="contentRootDir">The base directory to use for all requested assets, such as <c>wwwroot</c>.</param>
		/// <returns>Returns a <see cref="IFileProvider"/> for static assets, or <c>null</c> if there is no custom provider.</returns>
		public IFileProvider? CreateFileProvider(string contentRootDir);
	}
}
