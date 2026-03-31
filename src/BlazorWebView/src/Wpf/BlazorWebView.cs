// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.AspNetCore.Components.WebView.WebView2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using IWebView2 = Microsoft.Web.WebView2.Wpf.IWebView2;
using WebView2CompositionControl = Microsoft.Web.WebView2.Wpf.WebView2CompositionControl;
using WebView2Control = Microsoft.Web.WebView2.Wpf.WebView2;

namespace Microsoft.AspNetCore.Components.WebView.Wpf
{
	/// <summary>
	/// A Windows Presentation Foundation (WPF) control for hosting Razor components locally in Windows desktop applications.
	/// </summary>
	public class BlazorWebView : Control, IAsyncDisposable
	{
		#region Dependency property definitions
		/// <summary>
		/// The backing store for the <see cref="HostPage"/> property.
		/// </summary>
		public static readonly DependencyProperty HostPageProperty = DependencyProperty.Register(
			name: nameof(HostPage),
			propertyType: typeof(string),
			ownerType: typeof(BlazorWebView),
			typeMetadata: new PropertyMetadata(OnHostPagePropertyChanged));

		/// <summary>
		/// The backing store for the <see cref="StartPath"/> property.
		/// </summary>
		public static readonly DependencyProperty StartPathProperty = DependencyProperty.Register(
			name: nameof(StartPath),
			propertyType: typeof(string),
			ownerType: typeof(BlazorWebView),
			typeMetadata: new PropertyMetadata("/"));

		/// <summary>
		/// The backing store for the <see cref="RootComponent"/> property.
		/// </summary>
		public static readonly DependencyProperty RootComponentsProperty = DependencyProperty.Register(
			name: nameof(RootComponents),
			propertyType: typeof(RootComponentsCollection),
			ownerType: typeof(BlazorWebView));

		/// <summary>
		/// The backing store for the <see cref="Services"/> property.
		/// </summary>
		public static readonly DependencyProperty ServicesProperty = DependencyProperty.Register(
			name: nameof(Services),
			propertyType: typeof(IServiceProvider),
			ownerType: typeof(BlazorWebView),
			typeMetadata: new PropertyMetadata(OnServicesPropertyChanged));

		/// <summary>
		/// The backing store for the <see cref="UrlLoading"/> property.
		/// </summary>
		public static readonly DependencyProperty UrlLoadingProperty = DependencyProperty.Register(
			name: nameof(UrlLoading),
			propertyType: typeof(EventHandler<UrlLoadingEventArgs>),
			ownerType: typeof(BlazorWebView));

		/// <summary>
		/// The backing store for the <see cref="BlazorWebViewInitializing"/> event.
		/// </summary>
		public static readonly DependencyProperty BlazorWebViewInitializingProperty = DependencyProperty.Register(
			name: nameof(BlazorWebViewInitializing),
			propertyType: typeof(EventHandler<BlazorWebViewInitializingEventArgs>),
			ownerType: typeof(BlazorWebView));

		/// <summary>
		/// The backing store for the <see cref="BlazorWebViewInitialized"/> event.
		/// </summary>
		public static readonly DependencyProperty BlazorWebViewInitializedProperty = DependencyProperty.Register(
			name: nameof(BlazorWebViewInitialized),
			propertyType: typeof(EventHandler<BlazorWebViewInitializedEventArgs>),
			ownerType: typeof(BlazorWebView));

		/// <summary>
		/// The backing store for the <see cref="UseCompositionControl"/> property.
		/// </summary>
		public static readonly DependencyProperty UseCompositionControlProperty = DependencyProperty.Register(
			name: nameof(UseCompositionControl),
			propertyType: typeof(bool),
			ownerType: typeof(BlazorWebView),
			typeMetadata: new PropertyMetadata(true, OnUseCompositionControlPropertyChanged));

		#endregion

		private const string WebViewTemplateChildName = "WebView";
		private IWebView2? _webview;
		private WebView2WebViewManager? _webviewManager;
		private bool _isDisposed;

		static BlazorWebView()
		{
			// By default, prevent the BlazorWebView from receiving focus. Focus should typically be directed
			// to the underlying WebView2 control.
			FocusableProperty.OverrideMetadata(typeof(BlazorWebView), new FrameworkPropertyMetadata(false));

			// Listen for changes to the IsTabStop property so we can manipulate how tab navigation affects
			// the BlazorWebView's subtree.
			IsTabStopProperty.OverrideMetadata(typeof(BlazorWebView), new FrameworkPropertyMetadata(OnIsTabStopPropertyChanged));
		}

		/// <summary>
		/// Creates a new instance of <see cref="BlazorWebView"/>.
		/// </summary>
		public BlazorWebView()
		{
			ComponentsDispatcher = new WpfDispatcher(Application.Current.Dispatcher);

			SetValue(RootComponentsProperty, new RootComponentsCollection());
			RootComponents.CollectionChanged += HandleRootComponentsCollectionChanged;

			// Default to the composition control; OnUseCompositionControlPropertyChanged will
			// update this if UseCompositionControl is set to false before the control is initialized.
			Template = CreateWebViewTemplate(useComposition: true);

			ApplyTabNavigation(IsTabStop);
		}

		/// <summary>
		/// Returns the inner <see cref="IWebView2"/> used by this control.
		/// When <see cref="UseCompositionControl"/> is <see langword="true"/> (the default), this is a
		/// <see cref="WebView2CompositionControl"/>; otherwise it is a <see cref="WebView2Control"/>.
		/// </summary>
		/// <remarks>
		/// Directly using some functionality of the inner web view can cause unexpected results because its behavior
		/// is controlled by the <see cref="BlazorWebView"/> that is hosting it.
		/// </remarks>
		[Browsable(false)]
		public IWebView2 WebView => _webview!;

		/// <summary>
		/// Path to the host page within the application's static files. For example, <code>wwwroot\index.html</code>.
		/// This property must be set to a valid value for the Razor components to start.
		/// </summary>
		public string HostPage
		{
			get => (string)GetValue(HostPageProperty);
			set => SetValue(HostPageProperty, value);
		}

		/// <summary>
		/// Path for initial Blazor navigation when the Blazor component is finished loading.
		/// </summary>
		public string StartPath
		{
			get => (string)GetValue(StartPathProperty);
			set => SetValue(StartPathProperty, value);
		}

		/// <summary>
		/// A collection of <see cref="RootComponent"/> instances that specify the Blazor <see cref="IComponent"/> types
		/// to be used directly in the specified <see cref="HostPage"/>.
		/// </summary>
		public RootComponentsCollection RootComponents =>
			(RootComponentsCollection)GetValue(RootComponentsProperty);

		/// <summary>
		/// Allows customizing how links are opened.
		/// By default, opens internal links in the webview and external links in an external app.
		/// </summary>
		public EventHandler<UrlLoadingEventArgs> UrlLoading
		{
			get => (EventHandler<UrlLoadingEventArgs>)GetValue(UrlLoadingProperty);
			set => SetValue(UrlLoadingProperty, value);
		}

		/// <summary>
		/// Allows customizing the web view before it is created.
		/// </summary>
		public EventHandler<BlazorWebViewInitializingEventArgs> BlazorWebViewInitializing
		{
			get => (EventHandler<BlazorWebViewInitializingEventArgs>)GetValue(BlazorWebViewInitializingProperty);
			set => SetValue(BlazorWebViewInitializingProperty, value);
		}

		/// <summary>
		/// Allows customizing the web view after it is created.
		/// </summary>
		public EventHandler<BlazorWebViewInitializedEventArgs> BlazorWebViewInitialized
		{
			get => (EventHandler<BlazorWebViewInitializedEventArgs>)GetValue(BlazorWebViewInitializedProperty);
			set => SetValue(BlazorWebViewInitializedProperty, value);
		}

		/// <summary>
		/// Gets or sets an <see cref="IServiceProvider"/> containing services to be used by this control and also by application code.
		/// This property must be set to a valid value for the Razor components to start.
		/// </summary>
		public IServiceProvider Services
		{
			get => (IServiceProvider)GetValue(ServicesProperty);
			set => SetValue(ServicesProperty, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether to use the composition-based <see cref="WebView2CompositionControl"/>,
		/// which resolves WPF airspace issues at the cost of additional rendering overhead, or the standard
		/// <see cref="WebView2Control"/> for better performance in scenarios where airspace layering is not required.
		/// Defaults to <see langword="true"/>.
		/// </summary>
		/// <remarks>
		/// This property must be set before the control is initialized (e.g., in XAML or before adding the control to
		/// the visual tree). Changing it after the underlying WebView2 has been created will throw an
		/// <see cref="InvalidOperationException"/>.
		/// </remarks>
		public bool UseCompositionControl
		{
			get => (bool)GetValue(UseCompositionControlProperty);
			set => SetValue(UseCompositionControlProperty, value);
		}

		private static void OnServicesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((BlazorWebView)d).OnServicesPropertyChanged(e);

		private void OnServicesPropertyChanged(DependencyPropertyChangedEventArgs e) => StartWebViewCoreIfPossible();

		private static void OnHostPagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((BlazorWebView)d).OnHostPagePropertyChanged(e);

		private void OnHostPagePropertyChanged(DependencyPropertyChangedEventArgs e) => StartWebViewCoreIfPossible();

		private static void OnUseCompositionControlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((BlazorWebView)d).OnUseCompositionControlPropertyChanged(e);

		private void OnUseCompositionControlPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if (_webview != null)
			{
				throw new InvalidOperationException(
					$"The {nameof(UseCompositionControl)} property cannot be changed after the {nameof(BlazorWebView)} has been initialized.");
			}

			Template = CreateWebViewTemplate(useComposition: (bool)e.NewValue);
		}

		private static ControlTemplate CreateWebViewTemplate(bool useComposition)
		{
			var controlType = useComposition ? typeof(WebView2CompositionControl) : typeof(WebView2Control);
			return new ControlTemplate
			{
				VisualTree = new FrameworkElementFactory(controlType, WebViewTemplateChildName)
			};
		}

		private static void OnIsTabStopPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((BlazorWebView)d).OnIsTabStopPropertyChanged(e);

		private void OnIsTabStopPropertyChanged(DependencyPropertyChangedEventArgs e) => ApplyTabNavigation((bool)e.NewValue);

		private void ApplyTabNavigation(bool isTabStop)
		{
			var keyboardNavigationMode = isTabStop ? KeyboardNavigationMode.Local : KeyboardNavigationMode.None;
			KeyboardNavigation.SetTabNavigation(this, keyboardNavigationMode);
		}

		private bool RequiredStartupPropertiesSet =>
			_webview != null &&
			HostPage != null &&
			Services != null;

		/// <inheritdoc cref="FrameworkElement.OnApplyTemplate" />
		public override void OnApplyTemplate()
		{
			CheckDisposed();

			// Called when the control is created after its child control (the WebView2) is created from the Template property
			base.OnApplyTemplate();

			if (_webview == null)
			{
				_webview = (IWebView2)GetTemplateChild(WebViewTemplateChildName);
				StartWebViewCoreIfPossible();
			}
		}

		/// <inheritdoc cref="FrameworkElement.OnInitialized(EventArgs)" />
		protected override void OnInitialized(EventArgs e)
		{
			// Called when BeginInit/EndInit are used, such as when creating the control from XAML
			base.OnInitialized(e);
			StartWebViewCoreIfPossible();
		}

		private void StartWebViewCoreIfPossible()
		{
			CheckDisposed();

			if (!RequiredStartupPropertiesSet || _webviewManager != null)
			{
				return;
			}

			var logger = Services.GetService<ILogger<BlazorWebView>>() ?? NullLogger<BlazorWebView>.Instance;

			// We assume the host page is always in the root of the content directory, because it's
			// unclear there's any other use case. We can add more options later if so.
			string appRootDir;
#pragma warning disable IL3000 // 'System.Reflection.Assembly.Location.get' always returns an empty string for assemblies embedded in a single-file app. If the path to the app directory is needed, consider calling 'System.AppContext.BaseDirectory'.
			var entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
#pragma warning restore IL3000
			if (!string.IsNullOrEmpty(entryAssemblyLocation))
			{
				appRootDir = Path.GetDirectoryName(entryAssemblyLocation)!;
			}
			else
			{
				appRootDir = AppContext.BaseDirectory;
			}
			var hostPageFullPath = Path.GetFullPath(Path.Combine(appRootDir, HostPage));
			var contentRootDirFullPath = Path.GetDirectoryName(hostPageFullPath)!;
			var hostPageRelativePath = Path.GetRelativePath(contentRootDirFullPath, hostPageFullPath);
			var contentRootDirRelativePath = Path.GetRelativePath(appRootDir, contentRootDirFullPath);

			logger.CreatingFileProvider(contentRootDirFullPath, hostPageRelativePath);
			var fileProvider = CreateFileProvider(contentRootDirFullPath);

			_webviewManager = new WebView2WebViewManager(
				_webview!,
				Services,
				ComponentsDispatcher,
				fileProvider,
				RootComponents.JSComponents,
				contentRootDirRelativePath,
				hostPageRelativePath,
				(args) => UrlLoading?.Invoke(this, args),
				(args) => BlazorWebViewInitializing?.Invoke(this, args),
				(args) => BlazorWebViewInitialized?.Invoke(this, args),
				logger);

			StaticContentHotReloadManager.AttachToWebViewManagerIfEnabled(_webviewManager);

			foreach (var rootComponent in RootComponents)
			{
				logger.AddingRootComponent(rootComponent.ComponentType.FullName ?? string.Empty, rootComponent.Selector, rootComponent.Parameters?.Count ?? 0);

				// Since the page isn't loaded yet, this will always complete synchronously
				_ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
			}

			logger.StartingInitialNavigation(StartPath);
			_webviewManager.Navigate(StartPath);
		}

		private WpfDispatcher ComponentsDispatcher { get; }

		private void HandleRootComponentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
		{
			CheckDisposed();

			// If we haven't initialized yet, this is a no-op
			if (_webviewManager != null)
			{
				// Dispatch because this is going to be async, and we want to catch any errors
				_ = ComponentsDispatcher.InvokeAsync(async () =>
				{
					var newItems = (eventArgs.NewItems ?? Array.Empty<RootComponent>()).Cast<RootComponent>();
					var oldItems = (eventArgs.OldItems ?? Array.Empty<RootComponent>()).Cast<RootComponent>();

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
			else
			{
				// Typical case in development, as the files come from Microsoft.AspNetCore.Components.WebView.StaticContentProvider
				// instead and aren't copied to the bin dir
				return new NullFileProvider();
			}
		}

		/// <summary>
		/// Calls the specified <paramref name="workItem"/> asynchronously and passes in the scoped services available to Razor components.
		/// </summary>
		/// <param name="workItem">The action to call.</param>
		/// <returns>Returns a <see cref="Task"/> representing <c>true</c> if the <paramref name="workItem"/> was called, or <c>false</c> if it was not called because Blazor is not currently running.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="workItem"/> is <c>null</c>.</exception>
		public virtual async Task<bool> TryDispatchAsync(Action<IServiceProvider> workItem)
		{
			ArgumentNullException.ThrowIfNull(workItem);
			if (_webviewManager is null)
			{
				return false;
			}

			return await _webviewManager.TryDispatchAsync(workItem);
		}

		private void CheckDisposed()
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		/// <summary>
		/// Allows asynchronous disposal of the <see cref="BlazorWebView" />.
		/// </summary>
		protected virtual async ValueTask DisposeAsyncCore()
		{
			// Dispose this component's contents that user-written disposal logic and Razor component disposal logic will
			// complete first. Then dispose the WebView2 control. This order is critical because once the WebView2 is
			// disposed it will prevent and Razor component code from working because it requires the WebView to exist.
			if (_webviewManager != null)
			{
				await _webviewManager.DisposeAsync()
					.ConfigureAwait(false);
				_webviewManager = null;
			}

			(_webview as IDisposable)?.Dispose();
			_webview = null;
		}

		/// <inheritdoc />
		public async ValueTask DisposeAsync()
		{
			if (_isDisposed)
			{
				return;
			}
			_isDisposed = true;

			// Perform async cleanup.
			await DisposeAsyncCore();

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
			// Suppress finalization.
			GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
		}
	}
}
