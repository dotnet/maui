#nullable enable
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, ToggleSwitch>
	{
		readonly object _originalOnHoverColor;
		readonly WBrush _originalOnColorBrush;
		readonly WBrush _originalThumbOnBrush;

		protected override ToggleSwitch CreateNativeView() => new ToggleSwitch();

		public static void MapIsOn(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateIsToggled(view);
		}

		public static void MapTrackColor(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateTrackColor(view, handler._originalOnHoverColor, handler._originalOnColorBrush);
		}

		public static void MapThumbColor(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateThumbColor(view, handler._originalThumbOnBrush);
		}

		protected override void DisconnectHandler(ToggleSwitch nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView.Toggled -= OnToggled;
		}

		protected override void ConnectHandler(ToggleSwitch nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.Toggled += OnToggled;
		}

		void OnToggled(object sender, UI.Xaml.RoutedEventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			VirtualView.IsOn = NativeView.IsOn;
		}
	}
}