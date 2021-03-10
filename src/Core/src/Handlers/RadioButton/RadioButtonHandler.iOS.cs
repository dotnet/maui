using CoreAnimation;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : AbstractViewHandler<IRadioButton, UIButton>
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
			handler.TypedNativeView?.UpdateIsChecked(radioButton, RadioButtonLayer);
		}

		protected virtual CALayer CreateRadioBoxLayer()
		{
			return new RadioButtonCALayer(VirtualView, TypedNativeView);
		}

		void SetControlPropertiesFromProxy()
		{
			if (TypedNativeView == null)
				return;

			foreach (UIControlState uiControlState in ControlStates)
			{
				TypedNativeView.SetTitleColor(UIButton.Appearance.TitleColor(uiControlState), uiControlState); // If new values are null, old values are preserved.
				TypedNativeView.SetTitleShadowColor(UIButton.Appearance.TitleShadowColor(uiControlState), uiControlState);
				TypedNativeView.SetBackgroundImage(UIButton.Appearance.BackgroundImageForState(uiControlState), uiControlState);
			}
		}

		void SetRadioBoxLayer(CALayer layer)
		{
			RadioButtonLayer = layer;
			TypedNativeView?.Layer.AddSublayer(RadioButtonLayer);
			RadioButtonLayer.SetNeedsLayout();
			RadioButtonLayer.SetNeedsDisplay();
		}
	}
}