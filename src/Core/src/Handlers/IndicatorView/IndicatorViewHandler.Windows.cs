using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, UserControl>
	{
		protected override UserControl CreateNativeView() => throw new NotImplementedException();

		public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapSelectedIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapIndicatorShape(IndicatorViewHandler handler, IIndicatorView indicator) { }
	}
}
