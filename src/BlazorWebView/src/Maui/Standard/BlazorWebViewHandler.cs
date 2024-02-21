using System;
using System.Threading.Tasks;
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

		/// <summary>
		/// Calls the specified <paramref name="workItem"/> asynchronously and passes in the scoped services available to Razor components.
		/// </summary>
		/// <param name="workItem">The action to call.</param>
		/// <returns>Returns a <see cref="Task"/> representing <c>true</c> if the <paramref name="workItem"/> was called, or <c>false</c> if it was not called because Blazor is not currently running.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="workItem"/> is <c>null</c>.</exception>
		public virtual Task<bool> TryDispatchAsync(Action<IServiceProvider> workItem) => throw new NotSupportedException();
	}
}