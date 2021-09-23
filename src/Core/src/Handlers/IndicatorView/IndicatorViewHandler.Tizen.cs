using System;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, IndicatorView>
	{
		protected override IndicatorView CreateNativeView()
		{
			_ = NativeParent ?? throw new ArgumentNullException(nameof(NativeParent));
			return new IndicatorView(NativeParent);
		}

		protected override void ConnectHandler(IndicatorView nativeView)
		{
			base.ConnectHandler(nativeView);
			NativeView.SelectedPosition += OnSelectedPosition;
		}

		protected override void DisconnectHandler(IndicatorView nativeView)
		{
			base.DisconnectHandler(nativeView);
			NativeView.SelectedPosition -= OnSelectedPosition;
		}

		public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.UpdateIndicatorCount(indicator);
		}

		public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.UpdatePosition(indicator);
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