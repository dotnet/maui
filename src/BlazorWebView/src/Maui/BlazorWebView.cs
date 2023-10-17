using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui.Controls;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// A <see cref="View"/> that can render Blazor content.
	/// </summary>
	public partial class BlazorWebView : View, IBlazorWebView
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

		/// <inheritdoc />
		JSComponentConfigurationStore IBlazorWebView.JSComponents => _jSComponents;

		/// <summary>
		/// Gets or sets the path to the HTML file to render.
		/// <para>This is an app relative path to the file such as <c>wwwroot\index.html</c></para>
		/// </summary>
		public string? HostPage { get; set; }

		/// <summary>
		/// Bindable property for <see cref="StartPath"/>.
		/// </summary>
		public static readonly BindableProperty StartPathProperty = BindableProperty.Create(nameof(StartPath), typeof(string), typeof(BlazorWebView), "/");

		/// <summary>
		/// Gets or sets the path for initial navigation within the Blazor navigation context when the Blazor component is finished loading.
		/// </summary>
		public string StartPath
		{
			get { return (string)GetValue(StartPathProperty); }
			set { SetValue(StartPathProperty, value); }
		}

		/// <inheritdoc cref="IBlazorWebView.RootComponents" />
		public RootComponentsCollection RootComponents { get; }

		/// <summary>
		/// Allows customizing how links are opened.
		/// By default, opens internal links in the webview and external links in an external app.
		/// </summary>
		public event EventHandler<UrlLoadingEventArgs>? UrlLoading;

		/// <summary>
		/// Raised before the web view is initialized. On some platforms this enables customizing the web view configuration.
		/// </summary>
		public event EventHandler<BlazorWebViewInitializingEventArgs>? BlazorWebViewInitializing;

		/// <summary>
		/// Raised after the web view is initialized but before any component has been rendered. The event arguments provide the instance of the platform-specific web view control.
		/// </summary>
		public event EventHandler<BlazorWebViewInitializedEventArgs>? BlazorWebViewInitialized;

		/// <inheritdoc />
#if ANDROID
		[System.Runtime.Versioning.SupportedOSPlatform("android23.0")]
#elif IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios11.0")]
#endif
		public virtual IFileProvider CreateFileProvider(string contentRootDir)
		{
			// Call into the platform-specific code to get that platform's asset file provider
			return ((BlazorWebViewHandler)(Handler!)).CreateFileProvider(contentRootDir);
		}

		/// <summary>
		/// Calls the specified <paramref name="workItem"/> asynchronously and passes in the scoped services available to Razor components.
		/// </summary>
		/// <param name="workItem">The action to call.</param>
		/// <returns>Returns a <see cref="Task"/> representing <c>true</c> if the <paramref name="workItem"/> was called, or <c>false</c> if it was not called because Blazor is not currently running.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="workItem"/> is <c>null</c>.</exception>
#if ANDROID
		[System.Runtime.Versioning.SupportedOSPlatform("android23.0")]
#endif
		public virtual async Task<bool> TryDispatchAsync(Action<IServiceProvider> workItem)
		{
			ArgumentNullException.ThrowIfNull(workItem);
			if (Handler is null)
			{
				return false;
			}

			return await ((BlazorWebViewHandler)(Handler!)).TryDispatchAsync(workItem);
		}

		/// <inheritdoc />
		void IBlazorWebView.UrlLoading(UrlLoadingEventArgs args) =>
			UrlLoading?.Invoke(this, args);

		/// <inheritdoc />
		void IBlazorWebView.BlazorWebViewInitializing(BlazorWebViewInitializingEventArgs args) =>
			BlazorWebViewInitializing?.Invoke(this, args);

		/// <inheritdoc />
		void IBlazorWebView.BlazorWebViewInitialized(BlazorWebViewInitializedEventArgs args) =>
			BlazorWebViewInitialized?.Invoke(this, args);
	}
}
