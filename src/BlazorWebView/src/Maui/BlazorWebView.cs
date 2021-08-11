using System;
using Microsoft.Maui.Controls;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public class BlazorWebView : Microsoft.Maui.Controls.View, IBlazorWebView
	{
		public BlazorWebView()
		{
			RootComponents = new RootComponentsCollection(this);
		}

		public string? HostPage { get; set; }

		public RootComponentsCollection RootComponents { get; }

		new public BlazorWebViewHandler? Handler
		{
			get => base.Handler as BlazorWebViewHandler;
			set => base.Handler = value;
		}

		public WebViewManager? WebViewManager { get; internal set; }

		public event EventHandler<WebViewManagerCreatedEventArgs>? WebViewManagerCreated;

		protected override void OnHandlerChanging(HandlerChangingEventArgs args)
		{
			base.OnHandlerChanging(args);

			if (Handler != null)
			{
				Handler.WebViewManagerCreated -= OnHandlerWebViewManagerCreated;
			}

			WebViewManager = null;

			var newBlazorWebViewHandler = (BlazorWebViewHandler)args.NewHandler;
			newBlazorWebViewHandler.WebViewManagerCreated += OnHandlerWebViewManagerCreated;
		}

		private void OnHandlerWebViewManagerCreated(object? sender, WebViewManagerCreatedEventArgs e)
		{
			WebViewManager = e.WebViewManager;

			WebViewManagerCreated?.Invoke(this, e);
		}
	}
}
