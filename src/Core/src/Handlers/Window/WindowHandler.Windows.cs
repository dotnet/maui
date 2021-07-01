using System;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UI.Xaml.Window>
	{
		public static void MapTitle(WindowHandler handler, IWindow window) =>
			handler.NativeView?.UpdateTitle(window);

		public static void MapContent(WindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var nativeContent = window.Content.ToNative(handler.MauiContext);

			// TODO WINUI should this be some other known constant or via some mechanism? Or done differently?
			MauiWinUIApplication.Current.Resources.TryGetValue("MauiRootContainerStyle", out object? style);

			var root = new RootPanel
			{
				Style = style as UI.Xaml.Style,
				Children =
				{
					nativeContent
				}
			};

			handler.NativeView.Content = root;
		}
	}
}