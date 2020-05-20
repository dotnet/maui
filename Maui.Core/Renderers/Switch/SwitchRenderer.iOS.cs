using System;
using System.Drawing;
using UIKit;

namespace System.Maui.Platform
{
	public partial class SwitchRenderer : AbstractViewRenderer<ISwitch, UISwitch>
	{
		UIColor _defaultOnColor;
		UIColor _defaultThumbColor;

		protected override UISwitch CreateView()
		{
			var nativeView = new UISwitch(RectangleF.Empty);
			nativeView.ValueChanged += UISwitchValueChanged;
			return nativeView;
		}

		protected override void SetupDefaults()
		{
			_defaultOnColor = UISwitch.Appearance.OnTintColor;
			_defaultThumbColor = UISwitch.Appearance.ThumbTintColor;
			base.SetupDefaults();
		}

		protected override void DisposeView(UISwitch nativeView)
		{
			nativeView.ValueChanged -= UISwitchValueChanged;
			base.DisposeView(nativeView);
		}

		public virtual void UpdateIsOn()
		{
			TypedNativeView.SetState(VirtualView.IsOn, true);
		}

		public virtual void SetIsOn() =>
			VirtualView.IsOn = TypedNativeView.On;

		public virtual void UpdateOnColor()
		{
			var onColor = VirtualView.OnColor;

			TypedNativeView.OnTintColor = onColor.IsDefault ? _defaultOnColor : onColor.ToNativeColor();
		}

		public virtual void UpdateThumbColor()
		{
			var thumbColor = VirtualView.ThumbColor;
			TypedNativeView.ThumbTintColor = thumbColor.IsDefault ? _defaultThumbColor : thumbColor.ToNativeColor();
		}

		void UISwitchValueChanged(object sender, EventArgs e)
		{
			SetIsOn();
		}
	}
}
