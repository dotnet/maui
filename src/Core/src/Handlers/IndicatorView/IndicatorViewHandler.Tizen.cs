using System;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, NView>
	{
		protected override NView CreatePlatformView() => new NView
		{
			BackgroundColor = Tizen.NUI.Color.Red
		};


		public static void MapCount(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
		}

		public static void MapPosition(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
		}

		//TODO : Need to impl
		[MissingMapper]
		public static void MapHideSingle(IIndicatorViewHandler handler, IIndicatorView indicator) { }

		[MissingMapper]
		public static void MapMaximumVisible(IIndicatorViewHandler handler, IIndicatorView indicator) { }

		[MissingMapper]
		public static void MapIndicatorSize(IIndicatorViewHandler handler, IIndicatorView indicator) { }

		[MissingMapper]
		public static void MapIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator) { }

		[MissingMapper]
		public static void MapSelectedIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator) { }

		[MissingMapper]
		public static void MapIndicatorShape(IIndicatorViewHandler handler, IIndicatorView indicator) { }

	}
}