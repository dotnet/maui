using System;
using Android.Graphics.Drawables;
using Android.Nfc.CardEmulators;
using Android.Widget;
using Microsoft.Maui.Graphics;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, ASwitch>
	{
		CheckedChangeListener? _changeListener;
		protected override ASwitch CreatePlatformView()
		{
			return new ASwitch(Context);
		}

		protected override void ConnectHandler(ASwitch platformView)
		{
			_changeListener = new CheckedChangeListener(this);
			platformView.SetOnCheckedChangeListener(_changeListener);

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(ASwitch platformView)
		{
			platformView.SetOnCheckedChangeListener(null);
			_changeListener = null;

			base.DisconnectHandler(platformView);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			Size size = base.GetDesiredSize(widthConstraint, heightConstraint);

			if (size.Width == 0)
			{
				int width = (int)widthConstraint;

				if (widthConstraint <= 0)
					width = Context != null ? (int)Context.GetThemeAttributeDp(global::Android.Resource.Attribute.SwitchMinWidth) : 0;

				size = new Size(width, size.Height);
			}

			return size;
		}

		public static void MapIsOn(ISwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateIsOn(view);
		}

		public static void MapTrackColor(ISwitchHandler handler, ISwitch view)
		{
			if (handler is SwitchHandler platformHandler)
				handler.PlatformView?.UpdateTrackColor(view);
		}

		public static void MapThumbColor(ISwitchHandler handler, ISwitch view)
		{
			if (handler is SwitchHandler platformHandler)
				handler.PlatformView?.UpdateThumbColor(view);
		}

		void OnCheckedChanged(bool isOn)
		{
			if (VirtualView is null || VirtualView.IsOn == isOn)
				return;

			VirtualView.IsOn = isOn;
		}

		sealed class CheckedChangeListener : Java.Lang.Object, CompoundButton.IOnCheckedChangeListener
		{
			readonly WeakReference<SwitchHandler> _handler;

			public CheckedChangeListener(SwitchHandler handler)
			{
				_handler = new WeakReference<SwitchHandler>(handler);
			}

			void CompoundButton.IOnCheckedChangeListener.OnCheckedChanged(CompoundButton? buttonView, bool isToggled)
			{
				if (_handler.TryGetTarget(out var handler))
				{
					handler.OnCheckedChanged(isToggled);
				}
			}
		}
	}
}