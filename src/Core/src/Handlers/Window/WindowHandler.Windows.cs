using System;

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
		}

		protected override void DisconnectHandler(UI.Xaml.Window nativeView)
		{
			var windowManager = MauiContext?.GetNavigationRootManager();
			windowManager?.Connect(VirtualView.Content);

			_rootPanel?.Children?.Clear();
			nativeView.Content = null;

			base.DisconnectHandler(nativeView);
		}

		public static void MapTitle(WindowHandler handler, IWindow window) =>
			handler.NativeView?.UpdateTitle(window);

		public static void MapContent(WindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var windowManager = handler.MauiContext.GetNavigationRootManager();
			windowManager.Connect(handler.VirtualView.Content);
			handler?._rootPanel?.Children?.Clear();
			handler?._rootPanel?.Children?.Add(windowManager.RootView);
		}
	}
}