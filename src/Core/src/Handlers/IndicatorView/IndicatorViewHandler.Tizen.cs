using System;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, IndicatorView>
	{
		protected override IndicatorView CreatePlatformView()
		{
			_ = NativeParent ?? throw new ArgumentNullException(nameof(NativeParent));
			return new IndicatorView(NativeParent);
		}

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

		public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.UpdateIndicatorCount(indicator);
		}

		public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.UpdatePosition(indicator);
		}

		//TODO : Need to impl
		[MissingMapper]
		public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicator) { }

		[MissingMapper]
		public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicator) { }
		
		[MissingMapper]
		public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicator) { }

		[MissingMapper]
		public static void MapIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator) { }

		[MissingMapper]
		public static void MapSelectedIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator) { }

		[MissingMapper]
		public static void MapIndicatorShape(IndicatorViewHandler handler, IIndicatorView indicator) { }

		void OnSelectedPosition(object? sender, SelectedPositionChangedEventArgs e)
		{
			VirtualView.Position = e.SelectedPosition;
		}
	}
}