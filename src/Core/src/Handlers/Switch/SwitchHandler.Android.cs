using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, ASwitch>
	{
		CheckedChangeListener ChangeListener { get; } = new CheckedChangeListener();
		static ColorStateList? DefaultTrackColorStateList { get; set; }
		static ColorStateList? DefaultThumbColorStateList { get; set; }

		protected override ASwitch CreateNativeView()
		{
			return new ASwitch(Context);
		}

		protected override void ConnectHandler(ASwitch nativeView)
		{
			ChangeListener.Handler = this;
			nativeView.SetOnCheckedChangeListener(ChangeListener);
		}

		protected override void DisconnectHandler(ASwitch nativeView)
		{
			ChangeListener.Handler = null;
			nativeView.SetOnCheckedChangeListener(null);
		}

		protected override void SetupDefaults(ASwitch nativeView)
		{
			DefaultTrackColorStateList = nativeView.GetDefaultSwitchTrackColorStateList();
			DefaultThumbColorStateList = nativeView.GetDefaultSwitchThumbColorStateList();
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

		public static void MapIsOn(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateIsOn(view);
		}

		public static void MapTrackColor(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateTrackColor(view, DefaultTrackColorStateList);
		}

		public static void MapThumbColor(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateThumbColor(view, DefaultThumbColorStateList);
		}

		void OnCheckedChanged(bool isOn)
		{
			if (VirtualView == null)
				return;

			VirtualView.IsOn = isOn;
		}

		class CheckedChangeListener : Java.Lang.Object, CompoundButton.IOnCheckedChangeListener
		{
			public SwitchHandler? Handler { get; set; }

			void CompoundButton.IOnCheckedChangeListener.OnCheckedChanged(CompoundButton? buttonView, bool isToggled)
			{
				Handler?.OnCheckedChanged(isToggled);
			}
		}
	}
}