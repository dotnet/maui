using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, UIView>
	{
		protected override UIView CreateNativeView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(ContentView)}");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} cannot be null");

			var contentView = new ContentView();
			return contentView;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			NativeView.View = view;
		}

		public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
		{
		}

		public static void MapTextColor(RadioButtonHandler handler, ITextStyle textStyle)
		{
		}

		public static void MapCharacterSpacing(RadioButtonHandler handler, ITextStyle textStyle)
		{
		}

		public static void MapFont(RadioButtonHandler handler, ITextStyle textStyle)
		{
		}

		public static void MapContent(RadioButtonHandler handler, IRadioButton radioButton)
		{
		}
	}
}