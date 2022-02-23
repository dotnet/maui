#nullable enable
using Microsoft.UI.Xaml.Controls;
using WResourceDictionary = Microsoft.UI.Xaml.ResourceDictionary;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, ToggleSwitch>
	{
		WResourceDictionary? _originalResources;

		protected override ToggleSwitch CreatePlatformView() => new ToggleSwitch();

		void SetupDefaults(ToggleSwitch platformView)
		{
			_originalResources = platformView?.CloneResources();
		}

		public static void MapIsOn(SwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateIsToggled(view);
		}

		public static void MapTrackColor(SwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateTrackColor(view, handler._originalResources);
		}

		public static void MapThumbColor(SwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateThumbColor(view, handler._originalResources);
		}

		protected override void DisconnectHandler(ToggleSwitch platformView)
		{
			base.DisconnectHandler(platformView);
			platformView.Toggled -= OnToggled;
		}

		protected override void ConnectHandler(ToggleSwitch platformView)
		{
			base.ConnectHandler(platformView);
			SetupDefaults(platformView);
			platformView.Toggled += OnToggled;
		}

		void OnToggled(object sender, UI.Xaml.RoutedEventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.IsOn = PlatformView.IsOn;
		}
	}
}