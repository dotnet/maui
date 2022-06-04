using System;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, IndicatorView>
	{
		protected override IndicatorView CreatePlatformView() => new IndicatorView(PlatformParent);

		protected override void ConnectHandler(IndicatorView platformView)
		{
			base.ConnectHandler(platformView);
			PlatformView.SelectedPosition += OnSelectedPosition;
		}

		protected override void DisconnectHandler(IndicatorView platformView)
		{
			base.DisconnectHandler(platformView);
			PlatformView.SelectedPosition -= OnSelectedPosition;
		}

		public static void MapCount(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.UpdateIndicatorCount(indicator);
		}

		public static void MapPosition(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.UpdatePosition(indicator);
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

		void OnSelectedPosition(object? sender, SelectedPositionChangedEventArgs e)
		{
			VirtualView.Position = e.SelectedPosition;
		}
	}
}