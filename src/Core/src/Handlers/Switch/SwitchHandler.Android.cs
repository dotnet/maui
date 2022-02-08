using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui.Graphics;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, ASwitch>
	{
		Drawable? _defaultTrackDrawable;
		Drawable? _defaultThumbDrawable;

		CheckedChangeListener ChangeListener { get; } = new CheckedChangeListener();

		protected override ASwitch CreatePlatformView()
		{
			return new ASwitch(Context);
		}

		protected override void ConnectHandler(ASwitch platformView)
		{
			ChangeListener.Handler = this;
			platformView.SetOnCheckedChangeListener(ChangeListener);

			base.ConnectHandler(platformView);
			SetupDefaults(platformView);
		}

		protected override void DisconnectHandler(ASwitch platformView)
		{
			ChangeListener.Handler = null;
			platformView.SetOnCheckedChangeListener(null);

			_defaultTrackDrawable?.Dispose();
			_defaultTrackDrawable = null;

			_defaultThumbDrawable?.Dispose();
			_defaultThumbDrawable = null;

			base.DisconnectHandler(platformView);
		}

		void SetupDefaults(ASwitch platformView)
		{
			_defaultTrackDrawable = platformView.GetDefaultSwitchTrackDrawable();
			_defaultThumbDrawable = platformView.GetDefaultSwitchThumbDrawable();
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
			handler.PlatformView?.UpdateIsOn(view);
		}

		public static void MapTrackColor(SwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateTrackColor(view, handler._defaultTrackDrawable);
		}

		public static void MapThumbColor(SwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateThumbColor(view, handler._defaultThumbDrawable);
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