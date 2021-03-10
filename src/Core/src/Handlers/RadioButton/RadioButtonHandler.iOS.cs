using CoreAnimation;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : AbstractViewHandler<IRadioButton, UIButton>
	{
		static CALayer? RadioButtonLayer { get; set; }

		protected override UIButton CreateNativeView()
		{
			var nativeRadioButton = new UIButton(UIButtonType.System);

			SetRadioBoxLayer(CreateRadioBoxLayer());

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

		void SetRadioBoxLayer(CALayer layer)
		{
			RadioButtonLayer = layer;
			TypedNativeView?.Layer.AddSublayer(RadioButtonLayer);
			RadioButtonLayer.SetNeedsLayout();
			RadioButtonLayer.SetNeedsDisplay();
		}
	}
}