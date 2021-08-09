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

		Drawable? _currentPageShape;
		Drawable? _pageShape;

		protected override LinearLayout CreateNativeView() => new LinearLayout(Context);

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		public override void NativeArrange(Rectangle frame)
		{
			//	if (VirtualView.IndicatorTemplate != null)
			NativeView.Measure((int)frame.Width, (int)frame.Height);
			base.NativeArrange(frame);
		}

		private protected override void OnConnectHandler(View nativeView)
		{
			base.OnConnectHandler(nativeView);
		}

		public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UpdateItemsSource();
		}
		public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UpdateIndicators();
		}
		public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UpdateIndicatorCount();
		}
		public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UpdateIndicatorCount();
		}
		public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UpdateItemsSource();
		}

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

				NativeView.AddView(imageView);
			}

			for (int i = count; i < NativeView.ChildCount; i++)
			{
				NativeView.RemoveViewAt(NativeView.ChildCount - 1);
			}
		}

		void ResetIndicators()
		{
			_pageShape = null;
			_currentPageShape = null;

			var templatedIndicatorView = VirtualView as ITemplatedIndicatorView;
			if (templatedIndicatorView == null || templatedIndicatorView.IndicatorsLayoutOverride == null)
				UpdateShapes();
			else
				UpdateIndicatorTemplate(templatedIndicatorView.IndicatorsLayoutOverride);
		}

		void UpdateIndicatorTemplate(ILayout? layout)
		{
			if (layout == null)
				return;

			AView? handler;
			if (MauiContext != null)
			{
				handler = layout.ToNative(MauiContext);

				NativeView.RemoveAllViews();
				NativeView.AddView(handler);
			}
		}

		void UpdateIndicators()
		{
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
			if (_currentPageShape != null)
				return;

			var indicatorColor = VirtualView.IndicatorsColor;
			if (indicatorColor is SolidPaint indicatorPaint)
			{
				if (indicatorPaint.Color is Color c)
					_pageShape = GetShape(c.ToNative());

			}
			var indicatorPositionColor = VirtualView.PositionIndicatorColor;
			if (indicatorPositionColor is SolidPaint indicatorPositionPaint)
			{
				if (indicatorPositionPaint.Color is Color c)
					_currentPageShape = GetShape(c.ToNative());

			}
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
