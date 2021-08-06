using System;
using System.Linq;
using UIKit;
using NativeView = UIKit.UIView;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, NativeView>
	{
		protected override NativeView CreateNativeView()
		{
			return new UIView();
		}

		public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicator) { }
	}
}
