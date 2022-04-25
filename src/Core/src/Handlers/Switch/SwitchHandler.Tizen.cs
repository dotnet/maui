using System;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, Check>
	{
		protected override Check CreatePlatformView() => new Check(NativeParent)
		{
			Style = "toggle"
		};

		protected override void ConnectHandler(Check nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView!.StateChanged += OnStateChanged;
		}

		protected override void DisconnectHandler(Check nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView!.StateChanged -= OnStateChanged;
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
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.IsOn = PlatformView.IsChecked;
		}
	}
}