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
	public class LayoutViewGroup : ViewGroup, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable, IHandleWindowInsets
	{
		readonly ARect _clipRect = new();
		readonly Context _context;
		bool _didSafeAreaEdgeConfigurationChange;

		public bool InputTransparent { get; set; }

		public LayoutViewGroup(Context context) : base(context)
		{
			_context = context;
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

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			GlobalWindowInsetListenerExtensions.SetGlobalWindowInsetListener(this, _context);
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			GlobalWindowInsetListenerExtensions.RemoveGlobalWindowInsetListener(this, _context);
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

			// Account for padding in available space
			var paddingLeft = _context.FromPixels(PaddingLeft);
			var paddingTop = _context.FromPixels(PaddingTop);
			var paddingRight = _context.FromPixels(PaddingRight);
			var paddingBottom = _context.FromPixels(PaddingBottom);

			var availableWidth = Math.Max(0, deviceIndependentWidth - paddingLeft - paddingRight);
			var availableHeight = Math.Max(0, deviceIndependentHeight - paddingTop - paddingBottom);

			var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
			var heightMode = MeasureSpec.GetMode(heightMeasureSpec);
			var measure = CrossPlatformMeasure(availableWidth, availableHeight);

			// If the measure spec was exact, we should return the explicit size value, even if the content
			// measure came out to a different size
			var width = widthMode == MeasureSpecMode.Exactly ? deviceIndependentWidth : measure.Width + paddingLeft + paddingRight;
			var height = heightMode == MeasureSpecMode.Exactly ? deviceIndependentHeight : measure.Height + paddingTop + paddingBottom;

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

			var destination = _context.ToCrossPlatformRectInReferenceFrame(l, t, r, b);

			// Account for padding in layout bounds
			var paddingLeft = _context.FromPixels(PaddingLeft);
			var paddingTop = _context.FromPixels(PaddingTop);
			var paddingRight = _context.FromPixels(PaddingRight);
			var paddingBottom = _context.FromPixels(PaddingBottom);

			destination = new Rectangle(
				destination.X + paddingLeft,
				destination.Y + paddingTop,
				Math.Max(0, destination.Width - paddingLeft - paddingRight),
				Math.Max(0, destination.Height - paddingTop - paddingBottom));

			CrossPlatformArrange(destination);

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

			if (_didSafeAreaEdgeConfigurationChange)
			{
				InvalidateWindowInsets();
				_didSafeAreaEdgeConfigurationChange = false;
			}
		}

		/// <summary>
		/// Marks that the SafeAreaEdges configuration has changed and window insets must be
		/// reapplied on the next layout pass.
		/// </summary>
		internal void MarkSafeAreaEdgeConfigurationChanged()
		{
			_didSafeAreaEdgeConfigurationChange = true;
			RequestLayout();
		}

		#region IHandleWindowInsets Implementation

		(int left, int top, int right, int bottom) _originalPadding;
		bool _hasStoredOriginalPadding;


		/// <summary>
		/// Updates safe area configuration and triggers window insets re-application if needed.
		/// Call this when safe area edge configuration changes.
		/// </summary>
		internal void InvalidateWindowInsets()
		{
			// Reset descendants and request fresh insets to avoid double padding
			GlobalWindowInsetListenerExtensions.ResetDescendantsAndRequestInsets(this, _context);
		}

		public WindowInsetsCompat? HandleWindowInsets(View view, WindowInsetsCompat insets)
		{
			if (CrossPlatformLayout is null || insets is null)
			{
				return insets;
			}

			if (!_hasStoredOriginalPadding)
			{
				_originalPadding = (PaddingLeft, PaddingTop, PaddingRight, PaddingBottom);
				_hasStoredOriginalPadding = true;
			}

			if (GlobalWindowInsetListenerExtensions.IntersectsWithSystemBars(view, insets))
			{
				var processedInsets = SafeAreaExtensions.GetAdjustedSafeAreaInsets(insets, CrossPlatformLayout, _context);

				var pixelsLeft = (int)_context.ToPixels(processedInsets.Left);
				var pixelsTop = (int)_context.ToPixels(processedInsets.Top);
				var pixelsRight = (int)_context.ToPixels(processedInsets.Right);
				var pixelsBottom = (int)_context.ToPixels(processedInsets.Bottom);


				// Apply all insets to content view group
				SetPadding(pixelsLeft, pixelsTop, pixelsRight, pixelsBottom);

				if (processedInsets.Top > 0 || processedInsets.Bottom > 0 || processedInsets.Left > 0 || processedInsets.Right > 0)
				{
					// Consume all insets since we handled them
					return WindowInsetsCompat.Consumed;
				}
			}

			return insets;
		}

		public void ResetWindowInsets(View view)
		{
			if (_hasStoredOriginalPadding)
			{
				SetPadding(_originalPadding.left, _originalPadding.top, _originalPadding.right, _originalPadding.bottom);
			}
		}

		#endregion

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
	}
}
