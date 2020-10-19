using System;
#if __IOS__
using NativeView = UIKit.UILabel;
#elif __MACOS__
using NativeView = AppKit.NSTextField;
#elif MONOANDROID
using NativeView = Android.Widget.TextView;
#elif NETCOREAPP
using NativeView = System.Windows.Controls.TextBlock;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Xamarin.Platform.Handlers
{
	public class LabelHandler : AbstractViewHandler<ILabel, NativeView>
	{
		public static PropertyMapper<ILabel, LabelHandler> LabelMapper = new PropertyMapper<ILabel, LabelHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ILabel.Color)] = MapColor
		};

		public static void MapColor(LabelHandler handler, ILabel Label)
		{
		}

#if MONOANDROID
		protected override NativeView CreateNativeView() => new NativeView(this.Context);
#else
		protected override NativeView CreateNativeView() => new NativeView();
#endif

		public LabelHandler() : base(LabelMapper)
		{

		}

		public LabelHandler(PropertyMapper mapper) : base(mapper ?? LabelMapper)
		{

		}
	}
}