using System;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform.Android;
using static Android.Views.View;
using AColor = Android.Graphics.Color;
using AShapeDrawable = Android.Graphics.Drawables.ShapeDrawable;
using AShapes = Android.Graphics.Drawables.Shapes;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Handlers
{

	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, MauiPageControl>
	{
		protected override MauiPageControl CreateNativeView()
		{
			return new MauiPageControl(Context);
		}

		private protected override void OnConnectHandler(AView nativeView)
		{
			base.OnConnectHandler(nativeView);
			NativeView.SetIndicatorView(VirtualView);
		}

		private protected override void OnDisconnectHandler(AView nativeView)
		{
			base.OnDisconnectHandler(nativeView);
			NativeView.SetIndicatorView(null);
		}

		public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.UpdateIndicatorCount();
		}
		public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.UpdatePosition();
		}
		public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.UpdateIndicatorCount();
		}
		public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.UpdateIndicatorCount();
		}
		public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.ResetIndicators();
		}
		public static void MapIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.ResetIndicators();
		}
		public static void MapSelectedIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.ResetIndicators();
		}
		public static void MapIndicatorShape(IndicatorViewHandler handler, IIndicatorView indicator) 
		{
			handler.NativeView.ResetIndicators();
		}
	}
}
