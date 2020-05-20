#if __IOS__
using NativeView = UIKit.UIButton;
#elif __MACOS__
using NativeView = AppKit.NSButton;
#elif MONOANDROID
using NativeView = AndroidX.AppCompat.Widget.AppCompatButton;
#elif NETCOREAPP
using NativeView = System.Maui.Core.Controls.MauiButton;
#else
using NativeView = System.Object;
#endif

namespace System.Maui.Platform {
	public partial class ButtonRenderer : AbstractViewRenderer<IButton, NativeView>
	{
		public static Color DefaultTextColor { get; private set; }

		public static PropertyMapper<IButton> ButtonMapper = new PropertyMapper<IButton>(ViewRenderer.ViewMapper)
		{
			[nameof(IButton.Text)] = MapPropertyText,
			[nameof(IButton.Color)] = MapPropertyTextColor,
			//[nameof(IButton.Font)] = MapPropertyButtonFont,
			//[nameof(IButton.InputTransparent)] = MapPropertyButtonInputTransparent,
			//[nameof(IButton.CharacterSpacing)] = MapPropertyButtonCharacterSpacing
		};

		public ButtonRenderer () : base (ButtonMapper)
		{

		}
		public ButtonRenderer (PropertyMapper mapper) : base (mapper ?? ButtonMapper)
		{

		}

		public static void MapPropertyText(IViewRenderer renderer, IButton view)
		{
			var button = renderer.NativeView as NativeView;
			button.SetText(view.Text);
		}
		public static void MapPropertyTextColor(IViewRenderer renderer, IButton view)
		{
			var button = renderer.NativeView as NativeView;
			button.SetTextColor(view.Color, DefaultTextColor);
		}
	}
}
