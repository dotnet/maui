using Android.Graphics.Drawables;
using Android.Nfc.CardEmulators;
using Android.Widget;
using Microsoft.Maui.Graphics;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, ASwitch>
	{
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
		}

		protected override void DisconnectHandler(ASwitch platformView)
		{
			ChangeListener.Handler = null;
			platformView.SetOnCheckedChangeListener(null);

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

		class CheckedChangeListener : Java.Lang.Object, CompoundButton.IOnCheckedChangeListener
		{
			public SwitchHandler? Handler { get; set; }

			// Delay to allow the switch toggle animation to complete before updating shadow.
			// Android's SwitchCompat uses a built-in animation when toggling that can interfere
			// with shadow rendering if updated immediately. This delay ensures the animation
			// finishes before we re-apply the shadow to prevent visual glitches.
			private const int ShadowUpdateDelayMs = 150;

			void CompoundButton.IOnCheckedChangeListener.OnCheckedChanged(CompoundButton? buttonView, bool isToggled)
			{
				Handler?.OnCheckedChanged(isToggled);

				// Schedule shadow update after a short delay to allow toggle animation to finish
				if (Handler?.VirtualView?.Shadow is not null)
				{
					Handler.PlatformView?.PostDelayed(() => MapShadow(Handler, Handler.VirtualView), ShadowUpdateDelayMs);
				}
			}
		}
	}
}