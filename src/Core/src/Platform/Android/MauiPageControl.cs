using System;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Views.Accessibility;
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

		bool _isTemplateIndicator;
		
		static readonly IndicatorAccessibilityDelegate _accessibilityDelegate = new();

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
			_isTemplateIndicator = false;

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

				// Update accessibility information
				UpdateIndicatorAccessibility(view, i, index);
			}
		}

		public void UpdateIndicatorCount()
		{
			if (_indicatorView == null || Context == null || _isTemplateIndicator)
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

				// Set up accessibility for this indicator
				SetupIndicatorAccessibility(imageView, i, index);

				imageView.SetOnClickListener(new TEditClickListener(view =>
				{
					if (_indicatorView.IsEnabled && view?.Tag != null)
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
			_isTemplateIndicator = true;

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

		void SetupIndicatorAccessibility(ImageView imageView, int position, int selectedPosition)
		{
			if (_indicatorView is null)
			{
				return;
			}

			imageView.ImportantForAccessibility = ImportantForAccessibility.Yes;
			
			// Set accessibility delegate to prevent "button" announcement
			imageView.SetAccessibilityDelegate(_accessibilityDelegate);
			
			// Set the accessibility content description
			UpdateIndicatorAccessibility(imageView, position, selectedPosition);
		}

		void UpdateIndicatorAccessibility(ImageView imageView, int position, int selectedPosition)
		{
			if (_indicatorView is null)
			{
				return;
			}

			var itemNumber = position + 1;
			var totalItems = _indicatorView.GetMaximumVisible();
			var isSelected = position == selectedPosition;

			// Create descriptive content description for TalkBack
			var contentDescription = isSelected 
				? $"Item {itemNumber} of {totalItems}, selected"
				: $"Item {itemNumber} of {totalItems}";

			imageView.ContentDescription = contentDescription;
			
			// Prevent "double tap to activate" announcement for already selected indicators
			imageView.Clickable = !isSelected;
			
			// Force TalkBack to announce the updated description for the selected item
			if (isSelected && imageView.IsAccessibilityFocused)
			{
				// Send accessibility event to make TalkBack re-announce the updated content
				imageView.SendAccessibilityEvent(EventTypes.ViewAccessibilityFocused);
			}
		}

		AShapeDrawable? GetShape(AColor color)
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

			shape.Paint?.Color = color;
#pragma warning restore CA1416

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
			if (_isTemplateIndicator)
				return;

			var count = ChildCount;
			for (int i = startAt; i < count; i++)
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

		class IndicatorAccessibilityDelegate : AccessibilityDelegate
		{
			public override void OnInitializeAccessibilityNodeInfo(AView? host, AccessibilityNodeInfo? info)
			{
				if (host is null || info is null)
				{
					return;
				}
	
				base.OnInitializeAccessibilityNodeInfo(host, info);

				// Set class name to avoid "button" announcement
				info.ClassName = "android.view.View";
			}
		}
	}
}
