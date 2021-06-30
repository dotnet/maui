using System;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UI.Xaml.Window>
	{
		public static void MapTitle(WindowHandler handler, IWindow window) =>
			handler.NativeView?.UpdateTitle(window);

		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var nativeContent = VirtualView.Content.ToNative(MauiContext);

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

			NativeView.Content = root;
		}
	}
}