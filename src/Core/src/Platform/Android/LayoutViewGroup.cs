using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using Microsoft.Maui.Graphics;
using ARect = Android.Graphics.Rect;
using Rectangle = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Platform
{
	public class LayoutViewGroup : ViewGroup, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable
	{
		readonly ARect _clipRect = new();
		readonly Context _context;
		internal AndroidX.Core.Graphics.Insets? _currentInsets;
		public bool InputTransparent { get; set; }

		public LayoutViewGroup(Context context) : base(context)
		{
			_context = context;
			SetupWindowInsetsHandling();
		}

		public LayoutViewGroup(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			var context = Context;
			ArgumentNullException.ThrowIfNull(context);
			_context = context;
		}

		public LayoutViewGroup(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			_context = context;
		}

		public LayoutViewGroup(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			_context = context;
		}

		public LayoutViewGroup(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
		{
			_context = context;
		}

		public bool ClipsToBounds { get; set; }

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

		// TODO: Possibly reconcile this code with ViewHandlerExtensions.MeasureVirtualView
		// If you make changes here please review if those changes should also
		// apply to ViewHandlerExtensions.MeasureVirtualView
		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (CrossPlatformMeasure == null)
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

		// TODO: Possibly reconcile this code with ViewHandlerExtensions.MeasureVirtualView
		// If you make changes here please review if those changes should also
		// apply to ViewHandlerExtensions.MeasureVirtualView
		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (CrossPlatformArrange == null || _context == null)
			{
				return;
			}

			// Start with the full bounds of the container (edge-to-edge)
			var destination = _context.ToCrossPlatformRectInReferenceFrame(l, t, r, b);

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

			if (ClipsToBounds)
			{
				_clipRect.Right = r - l;
				_clipRect.Bottom = b - t;
				ClipBounds = _clipRect;
			}
			else
			{
				ClipBounds = null;
			}
		}

		public override bool OnTouchEvent(MotionEvent? e)
		{
			if (InputTransparent)
			{
				return false;
			}

			return base.OnTouchEvent(e);
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

		void SetupWindowInsetsHandling()
		{
			ViewCompat.SetOnApplyWindowInsetsListener(this, new LayoutWindowsListener());
		}

		internal void SetWindowInsets(AndroidX.Core.Graphics.Insets? insets)
		{
			if (_currentInsets != insets)
			{
				_currentInsets = insets;
				RequestLayout(); // Trigger a layout pass when insets change
			}
		}
	}

	internal class LayoutWindowsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
	{
		public WindowInsetsCompat? OnApplyWindowInsets(View? v, WindowInsetsCompat? insets)
		{
			if (insets is null || v is null || v is not LayoutViewGroup layoutViewGroup)
			{
				return insets;
			}

			var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
			var displayCutout = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());

			if (systemBars is null && displayCutout is null)
			{
				layoutViewGroup.SetWindowInsets(null);
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

			layoutViewGroup.SetWindowInsets(combinedInsets);

			// Clear any padding since we're handling insets through layout arrangement
			v.SetPadding(0, 0, 0, 0);

			// Consume the insets since we've handled them
			return WindowInsetsCompat.Consumed;
		}
	}
}
