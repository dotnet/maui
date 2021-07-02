using System;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, Check>
	{
		protected override Check CreateNativeView() => new Check(NativeParent)
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

		public static void MapIsOn(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateIsOn(view);
		}

		public static void MapTrackColor(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateTrackColor(view);
		}

		public static void MapThumbColor(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateThumbColor(view);
		}

		void OnStateChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			VirtualView.IsOn = NativeView.IsChecked;
		}
	}
}