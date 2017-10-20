using System;
using Android.Content;
using Android.Widget;
using ASwitch = Android.Widget.Switch;

namespace Xamarin.Forms.Platform.Android
{
	public class SwitchRenderer : ViewRenderer<Switch, ASwitch>, CompoundButton.IOnCheckedChangeListener
	{
		public SwitchRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use SwitchRenderer(Context) instead.")]
		public SwitchRenderer()
		{
			AutoPackage = false;
		}

		void CompoundButton.IOnCheckedChangeListener.OnCheckedChanged(CompoundButton buttonView, bool isChecked)
		{
			((IViewController)Element).SetValueFromRenderer(Switch.IsToggledProperty, isChecked);
		}

		public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			SizeRequest sizeConstraint = base.GetDesiredSize(widthConstraint, heightConstraint);

			if (sizeConstraint.Request.Width == 0)
			{
				int width = widthConstraint;
				if (widthConstraint <= 0)
					width = (int)Context.GetThemeAttributeDp(global::Android.Resource.Attribute.SwitchMinWidth);

				sizeConstraint = new SizeRequest(new Size(width, sizeConstraint.Request.Height), new Size(width, sizeConstraint.Minimum.Height));
			}

			return sizeConstraint;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && Control != null)
			{
				if (Element != null)
					Element.Toggled -= HandleToggled;

				Control.SetOnCheckedChangeListener(null);
			}

			base.Dispose(disposing);
		}

		protected override ASwitch CreateNativeControl()
		{
			return new ASwitch(Context);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
				e.OldElement.Toggled -= HandleToggled;

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var aswitch = CreateNativeControl();
					aswitch.SetOnCheckedChangeListener(this);
					SetNativeControl(aswitch);
				}
				else
					UpdateEnabled(); // Normally set by SetNativeControl, but not when the Control is reused.

				e.NewElement.Toggled += HandleToggled;
				Control.Checked = e.NewElement.IsToggled;
			}
		}

		void HandleToggled(object sender, EventArgs e)
		{
			Control.Checked = Element.IsToggled;
		}

		void UpdateEnabled()
		{
			Control.Enabled = Element.IsEnabled;
		}
	}
}