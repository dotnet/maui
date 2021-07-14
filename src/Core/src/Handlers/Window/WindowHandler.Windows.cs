using System;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UI.Xaml.Window>
	{
		RootPanel? rootPanel = null;

		protected override void ConnectHandler(UI.Xaml.Window nativeView)
		{
			base.ConnectHandler(nativeView);

			if (rootPanel == null)
			{
				// TODO WINUI should this be some other known constant or via some mechanism? Or done differently?
				MauiWinUIApplication.Current.Resources.TryGetValue("MauiRootContainerStyle", out object? style);

				rootPanel = new RootPanel
				{
					Style = style as UI.Xaml.Style
				};
			}

			nativeView.Content = rootPanel;
			nativeView.SizeChanged += NativeView_SizeChanged;
		}

		protected override void DisconnectHandler(UI.Xaml.Window nativeView)
		{
			nativeView.SizeChanged -= NativeView_SizeChanged;
			rootPanel?.Children?.Clear();
			nativeView.Content = null;

			base.DisconnectHandler(nativeView);
		}

		void NativeView_SizeChanged(object sender, UI.Xaml.WindowSizeChangedEventArgs args)
		{
			if (rootPanel != null)
			{
				foreach (var c in rootPanel.Children)
				{
					c.Measure(args.Size);
				}
			}
		}

		public static void MapTitle(WindowHandler handler, IWindow window) =>
			handler.NativeView?.UpdateTitle(window);

		public static void MapContent(WindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var nativeContent = window.Content.ToNative(handler.MauiContext);

			handler?.rootPanel?.Children?.Clear();
			handler?.rootPanel?.Children?.Add(nativeContent);
		}
	}
}