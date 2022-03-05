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

		public static void MapIsOn(ISwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateIsToggled(view);
		}

		public static void MapTrackColor(ISwitchHandler handler, ISwitch view)
		{
			if (handler is SwitchHandler platformHandler)
				handler.PlatformView?.UpdateTrackColor(view, platformHandler._originalResources);
		}

		public static void MapThumbColor(ISwitchHandler handler, ISwitch view)
		{
			if (handler is SwitchHandler platformHandler)
				handler.PlatformView?.UpdateThumbColor(view, platformHandler._originalResources);
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