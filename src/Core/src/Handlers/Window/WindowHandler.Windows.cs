using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UI.Xaml.Window>
	{
		RootPanel? _rootPanel = null;

		protected override void ConnectHandler(UI.Xaml.Window nativeView)
		{
			base.ConnectHandler(nativeView);

			if (_rootPanel == null)
			{
				// TODO WINUI should this be some other known constant or via some mechanism? Or done differently?
				MauiWinUIApplication.Current.Resources.TryGetValue("MauiRootContainerStyle", out object? style);

				_rootPanel = new RootPanel
				{
					Style = style as UI.Xaml.Style
				};
			}

			nativeView.Content = _rootPanel;

			var b = nativeView.Bounds;
			VirtualView.Frame = new Rectangle(b.Left, b.Top, b.Width, b.Height);

			nativeView.SizeChanged += OnWindowSizeChanged;
		}

		protected override void DisconnectHandler(UI.Xaml.Window nativeView)
		{
			MauiContext
				?.GetNavigationRootManager()
				?.Disconnect();

			_rootPanel?.Children?.Clear();
			nativeView.Content = null;
			nativeView.SizeChanged -= OnWindowSizeChanged;

			base.DisconnectHandler(nativeView);
		}

		public static void MapTitle(IWindowHandler handler, IWindow window) =>
			handler.NativeView?.UpdateTitle(window);

		public static void MapContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var windowManager = handler.MauiContext.GetNavigationRootManager();
			windowManager.Connect(handler.VirtualView.Content);
			var rootPanel = handler.NativeView.Content as Panel;

			if (rootPanel == null)
				return;

			rootPanel.Children.Clear();
			rootPanel.Children.Add(windowManager.RootView);

			if (window.VisualDiagnosticsOverlay != null)
				window.VisualDiagnosticsOverlay.Initialize();
		}

		public static void MapWidth(IWindowHandler handler, IWindow view) =>
			handler.NativeView?.UpdateWidth(view);

		public static void MapHeight(IWindowHandler handler, IWindow view) =>
			handler.NativeView?.UpdateHeight(view);

		public static void MapToolbar(IWindowHandler handler, IWindow view)
		{
			if (view is IToolbarElement tb)
				ViewHandler.MapToolbar(handler, tb);
		}

		void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"OnWindowSizeChanged: {NativeView.Bounds} {e.Size}");

			VirtualView.Frame = new Rectangle(0, 0, e.Size.Width, e.Size.Height);
		}
	}
}