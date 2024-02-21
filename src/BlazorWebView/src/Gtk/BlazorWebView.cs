using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Gtk;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Components.WebView.Gtk
{
	/// <summary>
	/// A Gtk Widget for hosting Razor components locally in Windows desktop applications.
	/// </summary>
	public class BlazorWebView : Bin
	{
		private readonly WebKit.WebView _webview;
		private GtkWebViewManager? _webviewManager;
		private string? _hostPage;
		private IServiceProvider? _services;

		/// <summary>
		/// Creates a new instance of <see cref="BlazorWebView"/>.
		/// </summary>
		public BlazorWebView()
		{
			Widgets = CreateWidgetsInstance();
			ComponentsDispatcher = Dispatcher.CreateDefault();

			RootComponents.CollectionChanged += HandleRootComponentsCollectionChanged;

			_webview = new WebKit.WebView();

			this.Child = _webview;
			((BlazorWebViewWidgetCollection)Widgets).AddInternal(_webview);
		}

		/// <summary>
		/// Returns the inner <see cref="WebView"/> used by this control.
		/// </summary>
		/// <remarks>
		/// Directly using some functionality of the inner web view can cause unexpected results because its behavior
		/// is controlled by the <see cref="BlazorWebView"/> that is hosting it.
		/// </remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public WebKit.WebView WebView => _webview;

		private Dispatcher ComponentsDispatcher { get; }

		WidgetCollection Widgets { get; set; }

		bool Created { get; set; }

		/// <inheritdoc />
		protected override void OnShown()
		{
			base.OnShown();
			Created = true;

			StartWebViewCoreIfPossible();
		}

		/// <summary>
		/// Path to the host page within the application's static files. For example, <code>wwwroot\index.html</code>.
		/// This property must be set to a valid value for the Razor components to start.
		/// </summary>
		[Category("Behavior")]
		[Description(@"Path to the host page within the application's static files. Example: wwwroot\index.html.")]
		public string? HostPage
		{
			get => _hostPage;
			set
			{
				_hostPage = value;
				OnHostPagePropertyChanged();
			}
		}

		// Learn more about these methods here: https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/defining-default-values-with-the-shouldserialize-and-reset-methods?view=netframeworkdesktop-4.8
		private void ResetHostPage() => HostPage = null;

		private bool ShouldSerializeHostPage() => !string.IsNullOrEmpty(HostPage);

		/// <summary>
		/// A collection of <see cref="RootComponent"/> instances that specify the Blazor <see cref="IComponent"/> types
		/// to be used directly in the specified <see cref="HostPage"/>.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public RootComponentsCollection RootComponents { get; } = new();

		/// <summary>
		/// Gets or sets an <see cref="IServiceProvider"/> containing services to be used by this control and also by application code.
		/// This property must be set to a valid value for the Razor components to start.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DisallowNull]
		public IServiceProvider Services
		{
			get => _services!;
			set
			{
				_services = value;
				OnServicesPropertyChanged();
			}
		}

		/// <summary>
		/// Allows customizing how links are opened.
		/// By default, opens internal links in the webview and external links in an external app.
		/// </summary>
		[Category("Action")] [Description("Allows customizing how links are opened. By default, opens internal links in the webview and external links in an external app.")]
		public EventHandler<UrlLoadingEventArgs>? UrlLoading;

		/// <summary>
		/// Allows customizing the web view before it is created.
		/// </summary>
		[Category("Action")] [Description("Allows customizing the web view before it is created.")]
		public EventHandler<BlazorWebViewInitializingEventArgs>? BlazorWebViewInitializing;

		/// <summary>
		/// Allows customizing the web view after it is created.
		/// </summary>
		[Category("Action")] [Description("Allows customizing the web view after it is created.")]
		public EventHandler<BlazorWebViewInitializedEventArgs>? BlazorWebViewInitialized;

		private void OnHostPagePropertyChanged() => StartWebViewCoreIfPossible();

		private void OnServicesPropertyChanged() => StartWebViewCoreIfPossible();

		private bool RequiredStartupPropertiesSet =>
			Created &&
			_webview != null &&
			HostPage != null &&
			Services != null;

		private void StartWebViewCoreIfPossible()
		{
			// We never start the Blazor code in design time because it doesn't make sense to run
			// a Razor component in the designer.
			// if (IsAncestorSiteInDesignMode)
			// {
			// 	return;
			// }

			// If we don't have all the required properties, or if there's already a WebViewManager, do nothing
			if (!RequiredStartupPropertiesSet || _webviewManager != null)
			{
				return;
			}

			// We assume the host page is always in the root of the content directory, because it's
			// unclear there's any other use case. We can add more options later if so.
			string appRootDir;
			var entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;

#if !DEBUG
			if (!string.IsNullOrEmpty(entryAssemblyLocation))
			{
				appRootDir = System.IO.Path.GetDirectoryName(entryAssemblyLocation)!;
			}
			else
#endif

			{
				appRootDir = Environment.CurrentDirectory;
			}

			var hostPageFullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(appRootDir, HostPage!)); // HostPage is nonnull because RequiredStartupPropertiesSet is checked above
			var contentRootDirFullPath = System.IO.Path.GetDirectoryName(hostPageFullPath)!;
			var contentRootRelativePath = System.IO.Path.GetRelativePath(appRootDir, contentRootDirFullPath);
			var hostPageRelativePath = System.IO.Path.GetRelativePath(contentRootDirFullPath, hostPageFullPath);

			var fileProvider = CreateFileProvider(contentRootDirFullPath);

			if (_webviewManager != null)
			{
				_webviewManager.DisposeAsync()
					.AsTask()
					.GetAwaiter()
					.GetResult();
				;
			}

			_webviewManager = new GtkWebViewManager(
				_webview,
				Services,
				ComponentsDispatcher,
				fileProvider,
				RootComponents.JSComponents,
				contentRootRelativePath,
				hostPageRelativePath,
				(args) => UrlLoading?.Invoke(this, args),
				(args) => BlazorWebViewInitializing?.Invoke(this, args),
				(args) => BlazorWebViewInitialized?.Invoke(this, args));

			StaticContentHotReloadManager.AttachToWebViewManagerIfEnabled(_webviewManager);

			foreach (var rootComponent in RootComponents)
			{
				// Since the page isn't loaded yet, this will always complete synchronously
				_ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
			}

			_webviewManager.Navigate("/");
		}

		private void HandleRootComponentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
		{
			// If we haven't initialized yet, this is a no-op
			if (_webviewManager != null)
			{
				// Dispatch because this is going to be async, and we want to catch any errors
				_ = ComponentsDispatcher.InvokeAsync(async () =>
				{
					var newItems = (eventArgs.NewItems ?? Array.Empty<object>()).Cast<RootComponent>();
					var oldItems = (eventArgs.OldItems ?? Array.Empty<object>()).Cast<RootComponent>();

					foreach (var item in newItems.Except(oldItems))
					{
						await item.AddToWebViewManagerAsync(_webviewManager);
					}

					foreach (var item in oldItems.Except(newItems))
					{
						await item.RemoveFromWebViewManagerAsync(_webviewManager);
					}
				});
			}
		}

		/// <summary>
		/// Creates a file provider for static assets used in the <see cref="BlazorWebView"/>. The default implementation
		/// serves files from disk. Override this method to return a custom <see cref="IFileProvider"/> to serve assets such
		/// as <c>wwwroot/index.html</c>. Call the base method and combine its return value with a <see cref="CompositeFileProvider"/>
		/// to use both custom assets and default assets.
		/// </summary>
		/// <param name="contentRootDir">The base directory to use for all requested assets, such as <c>wwwroot</c>.</param>
		/// <returns>Returns a <see cref="IFileProvider"/> for static assets.</returns>
		public virtual IFileProvider CreateFileProvider(string contentRootDir)
		{
			if (Directory.Exists(contentRootDir))
			{
				// Typical case after publishing, or if you're copying content to the bin dir in development for some nonstandard reason
				return new PhysicalFileProvider(contentRootDir);
			}

			// Typical case in development, as the files come from Microsoft.AspNetCore.Components.WebView.StaticContentProvider
			// instead and aren't copied to the bin dir
			return new NullFileProvider();
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Dispose this component's contents and block on completion so that user-written disposal logic and
				// Razor component disposal logic will complete first. Then call base.Dispose(), which will dispose
				// the WebView2 control. This order is critical because once the WebView2 is disposed it will prevent
				// Razor component code from working because it requires the WebView to exist.
				_webviewManager?
					.DisposeAsync()
					.AsTask()
					.GetAwaiter()
					.GetResult();
			}

			base.Dispose(disposing);
		}

		/// <inheritdoc />
		protected WidgetCollection CreateWidgetsInstance()
		{
			return new BlazorWebViewWidgetCollection(this);
		}

		/// <summary>
		/// Custom control collection that ensures that only the owning <see cref="BlazorWebView"/> can add
		/// controls to it.
		/// </summary>
		private sealed class BlazorWebViewWidgetCollection : WidgetCollection
		{
			public BlazorWebViewWidgetCollection(BlazorWebView owner) : base(owner) { }

			/// <summary>
			/// This is the only API we use; everything else is blocked.
			/// </summary>
			/// <param name="value"></param>
			internal void AddInternal(Widget value) => base.Add(value);

			// Everything below is overridden to protect the control collection as read-only.
			public override bool IsReadOnly => true;

			public override void Add(Widget? value) => throw new NotSupportedException();

			public override void Clear() => throw new NotSupportedException();

			public override void Remove(Widget? value) => throw new NotSupportedException();

			public override void SetChildIndex(Widget child, int newIndex) => throw new NotSupportedException();
		}
	}
}