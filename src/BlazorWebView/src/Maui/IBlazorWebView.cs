using System;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// Defines a contract for a view that renders Blazor content.
	/// </summary>
	public interface IBlazorWebView : IView, IWebRequestInterceptingWebView
	{
		/// <summary>
		/// Gets the path to the HTML file to render.
		/// </summary>
		string? HostPage { get; }

		/// <summary>
		/// Gets or sets the path for initial navigation within the Blazor navigation context when the Blazor component is finished loading.
		/// </summary>
		public string StartPath
		{
			get => "/";
			set => throw new NotImplementedException();
		}

		/// <summary>
		/// Gets a collection of <see cref="RootComponent"/> items.
		/// </summary>
		RootComponentsCollection RootComponents { get; }

		/// <summary>
		/// Gets the <see cref="JSComponentConfigurationStore"/>.
		/// </summary>
		JSComponentConfigurationStore JSComponents { get; }

		/// <summary>
		/// Creates a file provider for static assets used in the <see cref="BlazorWebView"/>. The default implementation
		/// serves files from a platform-specific location. Override this method to return a custom <see cref="IFileProvider"/> to serve assets such
		/// as <c>wwwroot/index.html</c>. Call the base method and combine its return value with a <see cref="CompositeFileProvider"/>
		/// to use both custom assets and default assets.
		/// </summary>
		/// <param name="contentRootDir">The base directory to use for all requested assets, such as <c>wwwroot</c>.</param>
		/// <returns>Returns a <see cref="IFileProvider"/> for static assets.</returns>
		IFileProvider CreateFileProvider(string contentRootDir);

		/// <summary>
		/// Notifies the control that the UrlLoading event should be raised with the specified <paramref name="args"/>.
		/// </summary>
		/// <param name="args">The arguments for the event.</param>
		void UrlLoading(UrlLoadingEventArgs args);

		/// <summary>
		/// Notifies the control that the BlazorWebViewInitializing event should be raised with the specified <paramref name="args"/>.
		/// </summary>
		/// <param name="args">The arguments for the event.</param>
		void BlazorWebViewInitializing(BlazorWebViewInitializingEventArgs args);

		/// <summary>
		/// Notifies the control that the BlazorWebViewInitialized event should be raised with the specified <paramref name="args"/>.
		/// </summary>
		/// <param name="args">The arguments for the event.</param>
		void BlazorWebViewInitialized(BlazorWebViewInitializedEventArgs args);
	}
}
