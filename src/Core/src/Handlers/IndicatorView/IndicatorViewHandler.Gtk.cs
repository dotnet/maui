using System;

namespace Microsoft.Maui.Handlers
{
	[MissingMapper]
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, NotImplementedView>
	{
		protected override NotImplementedView CreatePlatformView() => new();

		public static void MapCount(IIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapPosition(IIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapHideSingle(IIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapMaximumVisible(IIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapIndicatorSize(IIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapSelectedIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapIndicatorShape(IIndicatorViewHandler handler, IIndicatorView indicator) { }
	}
}