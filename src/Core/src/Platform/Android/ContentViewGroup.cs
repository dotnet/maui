using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public class ContentViewGroup : PlatformContentViewGroup, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable
	{
		IBorderStroke? _clip;
		readonly Context _context;
		internal AndroidX.Core.Graphics.Insets? _currentInsets;
		public ContentViewGroup(Context context) : base(context)
		{
			_context = context;
			SetupWindowInsetsHandling();
		}

		public ContentViewGroup(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			var context = Context;
			ArgumentNullException.ThrowIfNull(context);
			_context = context;
		}

		public ContentViewGroup(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			_context = context;
		}

		public ContentViewGroup(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			_context = context;
		}

		public ContentViewGroup(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
		{
			_context = context;
		}

		public ICrossPlatformLayout? CrossPlatformLayout
		{
			get; set;
		}

		Graphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Graphics.Size.Zero;
		}

		Graphics.Size CrossPlatformArrange(Graphics.Rect bounds)
		{
			return CrossPlatformLayout?.CrossPlatformArrange(bounds) ?? Graphics.Size.Zero;
		}

		void SetupWindowInsetsHandling()
		{
			ViewCompat.SetOnApplyWindowInsetsListener(this, new ContentWindowsListener());
		}

		internal void SetWindowInsets(AndroidX.Core.Graphics.Insets? insets)
		{
			if (_currentInsets != insets)
			{
				_currentInsets = insets;
				RequestLayout(); // Trigger a layout pass when insets change
			}
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (CrossPlatformLayout is null)
			{
				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
				return;
			}

			var deviceIndependentWidth = widthMeasureSpec.ToDouble(_context);
			var deviceIndependentHeight = heightMeasureSpec.ToDouble(_context);

			var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
			var heightMode = MeasureSpec.GetMode(heightMeasureSpec);

			// Always adjust measurement constraints for content when we have insets
			var measureWidth = deviceIndependentWidth;
			var measureHeight = deviceIndependentHeight;

			if (_currentInsets != null)
			{
				var leftInsetDp = _context.FromPixels(_currentInsets.Left);
				var topInsetDp = _context.FromPixels(_currentInsets.Top);
				var rightInsetDp = _context.FromPixels(_currentInsets.Right);
				var bottomInsetDp = _context.FromPixels(_currentInsets.Bottom);

				// Always reduce available space by insets for content measurement
				measureWidth = Math.Max(0, deviceIndependentWidth - leftInsetDp - rightInsetDp);
				measureHeight = Math.Max(0, deviceIndependentHeight - topInsetDp - bottomInsetDp);
			}

			var measure = CrossPlatformMeasure(measureWidth, measureHeight);

			// For the container size, always use original constraints in Exactly mode
			// In other modes, use the content size (which respects insets through arrangement)
			var width = widthMode == MeasureSpecMode.Exactly ? deviceIndependentWidth : measure.Width;
			var height = heightMode == MeasureSpecMode.Exactly ? deviceIndependentHeight : measure.Height;

			var platformWidth = _context.ToPixels(width);
			var platformHeight = _context.ToPixels(height);

			// Minimum values win over everything
			platformWidth = Math.Max(MinimumWidth, platformWidth);
			platformHeight = Math.Max(MinimumHeight, platformHeight);

			SetMeasuredDimension((int)platformWidth, (int)platformHeight);
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			if (CrossPlatformLayout is null)
			{
				return;
			}

			// Start with the full bounds of the container (edge-to-edge)
			var destination = _context.ToCrossPlatformRectInReferenceFrame(left, top, right, bottom);

			// If we have insets, adjust the content area to respect them
			if (_currentInsets != null)
			{
				var leftInsetDp = _context.FromPixels(_currentInsets.Left);
				var topInsetDp = _context.FromPixels(_currentInsets.Top);
				var rightInsetDp = _context.FromPixels(_currentInsets.Right);
				var bottomInsetDp = _context.FromPixels(_currentInsets.Bottom);

				// Create a new rect that is inset by the safe area amounts
				var safeDestination = new Graphics.Rect(
					destination.X + leftInsetDp,
					destination.Y + topInsetDp,
					destination.Width - leftInsetDp - rightInsetDp,
					destination.Height - topInsetDp - bottomInsetDp
				);

				// Arrange the content in the safe area
				CrossPlatformArrange(safeDestination);
			}
			else
			{
				// No insets, use full bounds
				CrossPlatformArrange(destination);
			}
		}

		internal IBorderStroke? Clip
		{
			get => _clip;
			set
			{
				_clip = value;
				// NOTE: calls PostInvalidate()
				SetHasClip(_clip is not null);
			}
		}

		protected override Path? GetClipPath(int width, int height)
		{
			if (Clip is null || Clip?.Shape is null)
				return null;

			float density = _context.GetDisplayDensity();
			float strokeThickness = (float)Clip.StrokeThickness;

			// We need to inset the content clipping by the width of the stroke on both sides
			// (top and bottom, left and right), so we remove it twice from the total width/height 
			var strokeInset = 2 * strokeThickness;
			float w = (width / density) - strokeInset;
			float h = (height / density) - strokeInset;
			float x = strokeThickness;
			float y = strokeThickness;
			IShape clipShape = Clip.Shape;

			var bounds = new Graphics.RectF(x, y, w, h);

			Path? platformPath = clipShape.ToPlatform(bounds, strokeThickness, density, true);
			return platformPath;
		}

		IVisualTreeElement? IVisualTreeElementProvidable.GetElement()
		{
			if (CrossPlatformLayout is IVisualTreeElement layoutElement &&
				layoutElement.IsThisMyPlatformView(this))
			{
				return layoutElement;
			}

			return null;
		}
	}

	internal class ContentWindowsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
	{
		public WindowInsetsCompat? OnApplyWindowInsets(View? v, WindowInsetsCompat? insets)
		{
			if (insets is null || v is null || v is not ContentViewGroup contentViewGroup)
			{
				return insets;
			}

			var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
			var displayCutout = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());

			if (systemBars is null && displayCutout is null)
			{
				contentViewGroup.SetWindowInsets(null);
				return insets;
			}

			// Calculate the maximum insets from system bars and display cutouts
			var leftInset = Math.Max(systemBars?.Left ?? 0, displayCutout?.Left ?? 0);
			var topInset = Math.Max(systemBars?.Top ?? 0, displayCutout?.Top ?? 0);
			var rightInset = Math.Max(systemBars?.Right ?? 0, displayCutout?.Right ?? 0);
			var bottomInset = Math.Max(systemBars?.Bottom ?? 0, displayCutout?.Bottom ?? 0);

			// Store the insets for use in layout arrangement
			var combinedInsets = AndroidX.Core.Graphics.Insets.Of(
				leftInset, topInset, rightInset, bottomInset);

			contentViewGroup.SetWindowInsets(combinedInsets);

			// Clear any padding since we're handling insets through layout arrangement
			v.SetPadding(0, 0, 0, 0);

			// Consume the insets since we've handled them
			return WindowInsetsCompat.Consumed;
		}
	}
}