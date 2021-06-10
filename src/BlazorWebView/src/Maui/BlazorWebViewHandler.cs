using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler
	{
		public static PropertyMapper<IBlazorWebView, BlazorWebViewHandler> BlazorWebViewMapper = new PropertyMapper<IBlazorWebView, BlazorWebViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IBlazorWebView.HostPage)] = MapHostPage,
			[nameof(IBlazorWebView.RootComponents)] = MapRootComponents,
		};

		public BlazorWebViewHandler() : base(BlazorWebViewMapper)
		{
		}

		public BlazorWebViewHandler(PropertyMapper mapper) : base(mapper ?? BlazorWebViewMapper)
		{
		}

		private string? HostPage { get; set; }

		private ObservableCollection<RootComponent>? _rootComponents;
		private ObservableCollection<RootComponent>? RootComponents
		{
			get => _rootComponents;
			set
			{
				if (_rootComponents != null)
				{
					// Remove any previously-known root components and unhook events
					_rootComponents.Clear();
					_rootComponents.CollectionChanged -= OnRootComponentsCollectionChanged;
				}

				_rootComponents = value;

				if (_rootComponents != null)
				{
					// Add new root components and hook events
					if (_rootComponents.Count > 0 && _webviewManager != null)
					{
						_webviewManager.Dispatcher.AssertAccess();
						foreach (var component in _rootComponents)
						{
							_ = component.AddToWebViewManagerAsync(_webviewManager);
						}
					}
					_rootComponents.CollectionChanged += OnRootComponentsCollectionChanged;
				}
			}
		}

		public static void MapHostPage(BlazorWebViewHandler handler, IBlazorWebView webView)
		{
			handler.HostPage = webView.HostPage;
			handler.StartWebViewCoreIfPossible();
		}

		public static void MapRootComponents(BlazorWebViewHandler handler, IBlazorWebView webView)
		{
			handler.RootComponents = webView.RootComponents;
			handler.StartWebViewCoreIfPossible();
		}

		private void OnRootComponentsCollectionChanged(object? sender, global::System.Collections.Specialized.NotifyCollectionChangedEventArgs eventArgs)
		{
			// If we haven't initialized yet, this is a no-op
			if (_webviewManager != null)
			{
				// Dispatch because this is going to be async, and we want to catch any errors
				_ = _webviewManager.Dispatcher.InvokeAsync(async () =>
				{
					var newItems = eventArgs.NewItems!.Cast<RootComponent>();
					var oldItems = eventArgs.OldItems!.Cast<RootComponent>();

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
	}
}
