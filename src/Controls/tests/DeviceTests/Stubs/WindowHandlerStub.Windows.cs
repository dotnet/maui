using System;
using Microsoft.Maui.Platform;
using WPanel = Microsoft.UI.Xaml.Controls.Panel;
using WWindow = Microsoft.UI.Xaml.Window;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public class WindowHandlerStub : ElementHandler<IWindow, UI.Xaml.Window>, IWindowHandler
	{
		public static IPropertyMapper<IWindow, WindowHandlerStub> WindowMapper = new PropertyMapper<IWindow, WindowHandlerStub>(WindowHandler.Mapper)
		{
			[nameof(IWindow.Content)] = MapContent
		};

		private static void MapContent(WindowHandlerStub handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var windowManager = handler.MauiContext.GetNavigationRootManager();
			windowManager.Connect(handler.VirtualView.Content);
			var rootPanel = handler.PlatformView.Content as WPanel;

			if (rootPanel == null)
				return;

			if (!rootPanel.Children.Contains(windowManager.RootView))
				rootPanel.Children.Add(windowManager.RootView);
		}

		protected override void DisconnectHandler(UI.Xaml.Window platformView)
		{
			var windowManager = MauiContext.GetNavigationRootManager();
			var rootPanel = platformView.Content as WPanel;
			rootPanel.Children.Remove(windowManager.RootView);
			windowManager.Disconnect();

			base.DisconnectHandler(platformView);
		}

		public WindowHandlerStub()
			: base(WindowMapper)
		{
		}

		protected override WWindow CreatePlatformElement()
		{
			return MauiProgram.CurrentWindow;
		}
	}
}
