using System;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// Defines the operations required by a handler for <see cref="IBlazorWebView"/>.
	/// </summary>
	public interface IBlazorWebViewHandler : IViewHandler
	{
		/// <summary>
		/// Creates the file provider used to serve static web assets.
		/// </summary>
		/// <param name="contentRootDir">The base directory for static web assets.</param>
		/// <returns>The file provider for the handler's platform.</returns>
		IFileProvider CreateFileProvider(string contentRootDir);

		/// <summary>
		/// Dispatches work to the scoped services available to Razor components.
		/// </summary>
		/// <param name="workItem">The work to dispatch.</param>
		/// <returns>
		/// A task representing <see langword="true"/> when the work was dispatched,
		/// or <see langword="false"/> when Blazor is not currently running.
		/// </returns>
		Task<bool> TryDispatchAsync(Action<IServiceProvider> workItem);
	}
}
