using System;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, UISwitch>
	{
		readonly SwitchProxy _proxy = new();

		// the UISwitch control becomes inaccessible if it grows to a width > 101
		// An issue has been logged with Apple
		// This ensures that the UISwitch remains the natural size that iOS expects
		// But the container can be used for setting BGColors and other features.
		public override bool NeedsContainer => true;

		protected override UISwitch CreatePlatformView()
		{
			return new UISwitch(RectangleF.Empty);
		}

		protected override void ConnectHandler(UISwitch platformView)
		{
			base.ConnectHandler(platformView);
			_proxy.Connect(VirtualView, platformView);
		}

		protected override void DisconnectHandler(UISwitch platformView)
		{
			base.DisconnectHandler(platformView);
			_proxy.Disconnect(platformView);
		}

		public static void MapIsOn(ISwitchHandler handler, ISwitch view)
		{
			UpdateIsOn(handler);
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

		static void UpdateIsOn(ISwitchHandler handler)
		{
			handler.UpdateValue(nameof(ISwitch.TrackColor));
		}

		class SwitchProxy
		{
			WeakReference<ISwitch>? _virtualView;

			ISwitch? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			public void Connect(ISwitch virtualView, UISwitch platformView)
			{
				_virtualView = new(virtualView);
				platformView.ValueChanged += OnControlValueChanged;
			}

			public void Disconnect(UISwitch platformView)
			{
				platformView.ValueChanged -= OnControlValueChanged;
			}

			void OnControlValueChanged(object? sender, EventArgs e)
			{
				if (VirtualView is ISwitch virtualView && sender is UISwitch platformView && virtualView.IsOn != platformView.On)
					virtualView.IsOn = platformView.On;
			}
		}
	}
}