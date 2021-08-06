using System;
using Android.Views;
using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using AShapeDrawable = Android.Graphics.Drawables.ShapeDrawable;
using AShapes = Android.Graphics.Drawables.Shapes;
using AShapeType = Android.Graphics.Drawables.ShapeType;
using AView = Android.Views.View;
using Android.Graphics;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, LinearLayout>
	{
		const int DefaultPadding = 4;

		Drawable? _currentPageShape = null;
		Drawable? _pageShape = null;

		protected override LinearLayout CreateNativeView()
		{
			return new LinearLayout(Context);
		}

		private protected override void OnConnectHandler(View nativeView)
		{
			base.OnConnectHandler(nativeView);
			NativeView.SetBackgroundColor(Colors.Red.ToNative());
			//NativeView.Layout(0, 0, 200, 200);
			UpdateItemsSource();

			UpdateIndicators();
			//UpdateHidesForSinglePage();
		}

		public override void NativeArrange(Rectangle frame)
		{
			base.NativeArrange(frame);
		
		}
		public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			//handler.UpdateItemsSource();
		}
		public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			//handler.UpdateIndicators();
		}
		public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			//handler.UpdateItemsSource();
		}
		public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicator) {
			//handler.UpdateItemsSource();
		}
		public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicator) { }

		void UpdateItemsSource()
		{
			ResetIndicators();
			UpdateIndicatorCount();
		}

		int GetIndexFromPosition()
		{
			var maxVisible = GetMaximumVisible();
			var position = VirtualView.Position;
			return Math.Max(0, position >= maxVisible ? maxVisible - 1 : position);
		}


		void UpdateIndicatorCount()
		{
			//if (!IsVisible)
			//	return;

			var index = GetIndexFromPosition();

			var count = GetMaximumVisible();

			var childCount = NativeView.ChildCount;

			for (int i = childCount; i < count; i++)
			{
				var imageView = new ImageView(Context);
				
				if (NativeView.Orientation == Orientation.Horizontal)
					imageView.SetPadding((int)Context.ToPixels(DefaultPadding), 0, (int)Context.ToPixels(DefaultPadding), 0);
				else
					imageView.SetPadding(0, (int)Context.ToPixels(DefaultPadding), 0, (int)Context.ToPixels(DefaultPadding));

				imageView.SetImageDrawable(index == i ? _currentPageShape : _pageShape);
			}

			childCount = NativeView.ChildCount;

			for (int i = count; i < childCount; i++)
			{
				NativeView.RemoveViewAt(NativeView.ChildCount - 1);
			}
			//IndicatorView.NativeSizeChanged();
			//VirtualView.InvalidateMeasure();
			//VirtualView.InvalidateArrange();
			NativeView.Invalidate();
		
		}

		void ResetIndicators()
		{
			//if (!IsVisible)
			//	return;

			//_pageIndicatorTintColor = IndicatorView.IndicatorColor.ToAndroid();
			//_currentPageIndicatorTintColor = IndicatorView.SelectedIndicatorColor.ToAndroid();
			//_shapeType = IndicatorView.IndicatorsShape == IndicatorShape.Circle ? AShapeType.Oval : AShapeType.Rectangle;
			_pageShape = null;
			_currentPageShape = null;

			//if (IndicatorView.IndicatorTemplate == null)
			UpdateShapes();
			//else
			//	UpdateIndicatorTemplate();

			UpdateIndicators();
		}

		void UpdateIndicators()
		{
			//if (!IsVisible)
			//	return;
			var index = GetIndexFromPosition();
			var count = NativeView.ChildCount;
			for (int i = 0; i < count; i++)
			{
				ImageView? view = NativeView.GetChildAt(i) as ImageView;
				if (view == null)
					continue;
				var drawableToUse = index == i ? _currentPageShape : _pageShape;
				if (drawableToUse != view.Drawable)
					view.SetImageDrawable(drawableToUse);
			}
		}

		void UpdateShapes()
		{
			//if (_currentPageShape != null)
			//	return;
			//var indicatorColor = VirtualView.IndicatorsColor;
			//if (indicatorColor is SolidPaint solidPaint)
			//{
			//	if (solidPaint.Color is Color c)
			//		_currentPageShape = GetShape(c.ToNative());

			//}
			//var indicatorPositionColor = VirtualView.PositionIndicatorColor;
			//if (indicatorColor is SolidPaint solidPaint1)
			//{
			//	if (solidPaint1.Color is Color c)
			//		_pageShape = GetShape(c.ToNative());

			//}

			_currentPageShape = GetShape(Colors.Black.ToNative());
			_pageShape = GetShape(Colors.Gray.ToNative());
		}

		Drawable GetShape(AColor color)
		{
			var indicatorSize = VirtualView.IndicatorSize;
			AShapeDrawable shape;

			//if (_shapeType == AShapeType.Oval)
				shape = new AShapeDrawable(new AShapes.OvalShape());
			//else
			//shape = new AShapeDrawable(new AShapes.RectShape());

			shape.SetIntrinsicHeight((int)Context.ToPixels(indicatorSize));
			shape.SetIntrinsicWidth((int)Context.ToPixels(indicatorSize));
			if (shape.Paint != null)
				shape.Paint.Color = color;

			return shape;
		}

		int GetMaximumVisible()
		{
			var minValue = Math.Min(VirtualView.MaximumVisible, VirtualView.Count);
			var maximumVisible = minValue <= 0 ? 0 : minValue;
			bool hideSingle = VirtualView.HideSingle;

			if (maximumVisible == 1 && hideSingle)
				maximumVisible = 0;

			return maximumVisible;
		}
	}
}
