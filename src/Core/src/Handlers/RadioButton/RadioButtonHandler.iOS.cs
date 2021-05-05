using CoreAnimation;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, UIButton>
	{
		static readonly UIControlState[] ControlStates = { UIControlState.Normal, UIControlState.Highlighted, UIControlState.Disabled };

		static CALayer? RadioButtonLayer { get; set; }

		protected override UIButton CreateNativeView()
		{
			var nativeRadioButton = new UIButton(UIButtonType.System);

			SetRadioBoxLayer(CreateRadioBoxLayer());
			SetControlPropertiesFromProxy();

			nativeRadioButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;

			return nativeRadioButton;
		}

		public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
		{
			handler.NativeView?.UpdateIsChecked(radioButton, RadioButtonLayer);
		}

		protected virtual CALayer CreateRadioBoxLayer()
		{
			return new RadioButtonCALayer(VirtualView, NativeView);
		}

		void SetControlPropertiesFromProxy()
		{
			if (NativeView == null)
				return;

			foreach (UIControlState uiControlState in ControlStates)
			{
				NativeView.SetTitleColor(UIButton.Appearance.TitleColor(uiControlState), uiControlState); // If new values are null, old values are preserved.
				NativeView.SetTitleShadowColor(UIButton.Appearance.TitleShadowColor(uiControlState), uiControlState);
				NativeView.SetBackgroundImage(UIButton.Appearance.BackgroundImageForState(uiControlState), uiControlState);
			}
		}

		void SetRadioBoxLayer(CALayer layer)
		{
			RadioButtonLayer = layer;
			NativeView?.Layer.AddSublayer(RadioButtonLayer);
			RadioButtonLayer.SetNeedsLayout();
			RadioButtonLayer.SetNeedsDisplay();
		}
	}
}