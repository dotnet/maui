#nullable enable
using Microsoft.UI.Xaml.Controls;
using WResourceDictionary = Microsoft.UI.Xaml.ResourceDictionary;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, ToggleSwitch>
	{
		WResourceDictionary? _originalResources;

		protected override ToggleSwitch CreateNativeView() => new ToggleSwitch();

		void SetupDefaults(ToggleSwitch nativeView)
		{
			_originalResources = nativeView?.CloneResources();
		}

		public static void MapIsOn(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateIsToggled(view);
		}

		public static void MapTrackColor(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateTrackColor(view, handler._originalResources);
		}

		public static void MapThumbColor(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateThumbColor(view, handler._originalResources);
		}

		protected override void DisconnectHandler(ToggleSwitch nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView.Toggled -= OnToggled;
		}

		protected override void ConnectHandler(ToggleSwitch nativeView)
		{
			base.ConnectHandler(nativeView);
			SetupDefaults(nativeView);
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