using System;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, UISwitch>
	{
		protected override UISwitch CreatePlatformView()
		{
			return new UISwitch(RectangleF.Empty);
		}

		protected override void ConnectHandler(UISwitch platformView)
		{
			base.ConnectHandler(platformView);

			platformView.ValueChanged += OnControlValueChanged;
		}

		protected override void DisconnectHandler(UISwitch platformView)
		{
			base.DisconnectHandler(platformView);

			platformView.ValueChanged -= OnControlValueChanged;
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

		void OnControlValueChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null)
				return;

			if (PlatformView != null)
				VirtualView.IsOn = PlatformView.On;
		}
	}
}