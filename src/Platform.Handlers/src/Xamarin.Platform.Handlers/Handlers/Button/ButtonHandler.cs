using System;
using Xamarin.Forms;

#if __IOS__
using NativeView = UIKit.UIButton;
#elif __MACOS__
using NativeView = AppKit.NSButton;
#elif MONOANDROID
using NativeView = AndroidX.AppCompat.Widget.AppCompatButton;
#elif NETCOREAPP
using NativeView = System.Windows.Controls.Button;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Xamarin.Platform.Handlers
{
	public class ButtonHandler : AbstractViewHandler<IButton, NativeView>
	{
		public static PropertyMapper<IButton, ButtonHandler> ButtonMapper = new PropertyMapper<IButton, ButtonHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IButton.Text)] = MapText,
			[nameof(IButton.Color)] = MapColor
		};

		public static void MapColor(ButtonHandler handler, IButton button)
		{
			ViewHandler.CheckParameters(handler, button);
			handler.TypedNativeView?.UpdateColor(button);
		}

		public static void MapText(ButtonHandler handler, IButton button)
		{
			ViewHandler.CheckParameters(handler, button);
			handler.TypedNativeView?.UpdateText(button);
		}

#if MONOANDROID
		protected override NativeView CreateView() => new NativeView(this.Context);
#else
		protected override NativeView CreateView() => new NativeView();
#endif

		public ButtonHandler() : base(ButtonMapper)
		{

		}

		public ButtonHandler(PropertyMapper mapper) : base(mapper ?? ButtonMapper)
		{

		}
	}
}