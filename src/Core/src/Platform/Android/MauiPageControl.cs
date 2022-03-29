using System;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using AShapeDrawable = Android.Graphics.Drawables.ShapeDrawable;
using AShapes = Android.Graphics.Drawables.Shapes;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Platform
{
	public class MauiPageControl : LinearLayout
	{
		const int DefaultPadding = 4;

		Drawable? _currentPageShape;
		Drawable? _pageShape;

		IIndicatorView? _indicatorView;

		public MauiPageControl(Context? context) : base(context)
		{
		}

		public void SetIndicatorView(IIndicatorView? indicatorView)
		{
			_indicatorView = indicatorView;
			if (indicatorView == null)
			{
				RemoveViews(0);
			}
		}

		public void ResetIndicators()
		{
			_pageShape = null;
			_currentPageShape = null;

			if ((_indicatorView as ITemplatedIndicatorView)?.IndicatorsLayoutOverride == null)
				UpdateShapes();
			else
				UpdateIndicatorTemplate((_indicatorView as ITemplatedIndicatorView)?.IndicatorsLayoutOverride);

			UpdatePosition();
		}

		public void UpdatePosition()
		{
			var index = GetIndexFromPosition();
			var count = ChildCount;
			for (int i = 0; i < count; i++)
			{
				ImageView? view = GetChildAt(i) as ImageView;
				if (view == null)
					continue;
				var drawableToUse = index == i ? _currentPageShape : _pageShape;
				if (drawableToUse != view.Drawable)
					view.SetImageDrawable(drawableToUse);
			}
		}

		public void UpdateIndicatorCount()
		{
			if (_indicatorView == null || Context == null)
				return;

			var index = GetIndexFromPosition();

			var count = _indicatorView.GetMaximumVisible();

			var childCount = ChildCount;

			for (int i = childCount; i < count; i++)
			{
				var imageView = new ImageView(Context)
				{
					Tag = i
				};

				if (Orientation == Orientation.Horizontal)
					imageView.SetPadding((int)Context.ToPixels(DefaultPadding), 0, (int)Context.ToPixels(DefaultPadding), 0);
				else
					imageView.SetPadding(0, (int)Context.ToPixels(DefaultPadding), 0, (int)Context.ToPixels(DefaultPadding));

				imageView.SetImageDrawable(index == i ? _currentPageShape : _pageShape);

				imageView.SetOnClickListener(new TEditClickListener(view =>
				{
					if (view?.Tag != null)
					{
						var position = (int)view.Tag;
						_indicatorView.Position = position;
					}
				}));

				AddView(imageView);
			}

			RemoveViews(count);
		}

		void UpdateIndicatorTemplate(ILayout? layout)
		{
			if (layout == null || _indicatorView?.Handler?.MauiContext == null)
				return;

			AView? handler = layout.ToPlatform(_indicatorView.Handler.MauiContext);

			RemoveAllViews();
			AddView(handler);
		}

		void UpdateShapes()
		{
			if (_currentPageShape != null || _indicatorView == null)
				return;

			var indicatorColor = _indicatorView.IndicatorColor;

			if (indicatorColor is SolidPaint indicatorPaint)
			{
				if (indicatorPaint.Color is Color c)
					_pageShape = GetShape(c.ToPlatform());

			}
			var indicatorPositionColor = _indicatorView.SelectedIndicatorColor;
			if (indicatorPositionColor is SolidPaint indicatorPositionPaint)
			{
				if (indicatorPositionPaint.Color is Color c)
					_currentPageShape = GetShape(c.ToPlatform());
			}
		}

		Drawable? GetShape(AColor color)
		{
			if (_indicatorView == null || Context == null)
				return null;

			AShapeDrawable shape;

			if (_indicatorView.IsCircleShape())
				shape = new AShapeDrawable(new AShapes.OvalShape());
			else
				shape = new AShapeDrawable(new AShapes.RectShape());

			var indicatorSize = _indicatorView.IndicatorSize;

			shape.SetIntrinsicHeight((int)Context.ToPixels(indicatorSize));
			shape.SetIntrinsicWidth((int)Context.ToPixels(indicatorSize));

			if (shape.Paint != null && OperatingSystem.IsAndroidVersionAtLeast(29))
				shape.Paint.Color = color;

			return shape;
		}

		int GetIndexFromPosition()
		{
			if (_indicatorView == null)
				return 0;

			var maxVisible = _indicatorView.GetMaximumVisible();
			var position = _indicatorView.Position;
			return Math.Max(0, position >= maxVisible ? maxVisible - 1 : position);
		}

		void RemoveViews(int startAt)
		{
			for (int i = startAt; i < ChildCount; i++)
			{
				var imageView = GetChildAt(ChildCount - 1);
				imageView?.SetOnClickListener(null);
				RemoveView(imageView);
			}
		}

		class TEditClickListener : Java.Lang.Object, IOnClickListener
		{
			Action<AView?>? _command;

			public TEditClickListener(Action<AView?> command)
			{
				_command = command;
			}

			public void OnClick(AView? v)
			{
				_command?.Invoke(v);
			}
			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_command = null;
				}
				base.Dispose(disposing);
			}
		}
	}
}
