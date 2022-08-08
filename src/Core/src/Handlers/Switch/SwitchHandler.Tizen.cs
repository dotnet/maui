using System;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, Check>
	{
		protected override Check CreatePlatformView() => new Check(PlatformParent)
		{
			Style = "toggle"
		};

		protected override void ConnectHandler(Check platformView)
		{
			base.ConnectHandler(platformView);
			platformView!.StateChanged += OnStateChanged;
		}

		protected override void DisconnectHandler(Check platformView)
		{
			base.DisconnectHandler(platformView);
			platformView!.StateChanged -= OnStateChanged;
		}

		public static void MapIsOn(ISwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateIsOn(view);
		}

		public static void MapTrackColor(ISwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateTrackColor(view);
		}

		public static void MapThumbColor(ISwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateThumbColor(view);
		}

		void OnStateChanged(object? sender, EventArgs e)
		{
			if (VirtualView is null || PlatformView is null || VirtualView.IsOn == PlatformView.IsChecked)
				return;

			VirtualView.IsOn = PlatformView.IsChecked;
		}
	}
}