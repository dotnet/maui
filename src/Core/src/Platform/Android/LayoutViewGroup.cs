using System;
using Android.Content;
using Android.Content.Res;
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
	public class LayoutViewGroup : PlatformViewGroup, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable, IHandleWindowInsets
	{
		readonly ARect _clipRect = new();
		readonly Context _context;
		bool _didSafeAreaEdgeConfigurationChange = true;
		bool _isInsetListenerSet;
		ViewOutlineProvider? _originalOutlineProvider;
		bool _outlineProviderSaved;

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
			_isInsetListenerSet = MauiWindowInsetListenerExtensions.TrySetMauiWindowInsetListener(this, _context);
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			if (_isInsetListenerSet)
				MauiWindowInsetListenerExtensions.RemoveMauiWindowInsetListener(this, _context);

			_didSafeAreaEdgeConfigurationChange = true;
			_isInsetListenerSet = false;
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
				TranslationZ = 0f;

				if (_outlineProviderSaved)
				{
					OutlineProvider = _originalOutlineProvider;
					_outlineProviderSaved = false;
				}
			}
			else
			{
				ClipBounds = null;

				// When children are allowed to overflow this layout's bounds, we raise the
				// translationZ of this view so that its overflowing children render on top of
				// any sibling views that would otherwise be drawn over them due to z-order.
				// Convert the content area to physical pixels for an exact integer comparison
				// that avoids floating-point rounding artefacts from the DIU→pixel round-trip.
				int contentWidthPx = (int)_context.ToPixels(destination.Width);
				int contentHeightPx = (int)_context.ToPixels(destination.Height);
				
				if (HasChildrenOutsideBounds(contentWidthPx, contentHeightPx))
				{
					if (!_outlineProviderSaved)
					{
						_originalOutlineProvider = OutlineProvider;
						_outlineProviderSaved = true;
					}
					OutlineProvider = null;  // Prevent elevation shadow from background
					TranslationZ = 1f;
				}
				else
				{
					if (_outlineProviderSaved)
					{
						OutlineProvider = _originalOutlineProvider;
						_outlineProviderSaved = false;
					}
					TranslationZ = 0f;
				}
			}

			if (_didSafeAreaEdgeConfigurationChange && _isInsetListenerSet)
			{
				ViewCompat.RequestApplyInsets(this);
				_didSafeAreaEdgeConfigurationChange = false;
			}
		}

		/// <summary>
		/// Determines whether any child view within this layout is positioned outside the
		/// layout's bounds. Used to decide whether to raise translationZ so that overflowing
		/// children render on top of sibling views.
		/// </summary>
		bool HasChildrenOutsideBounds(int widthPx, int heightPx)
		{
			if (CrossPlatformLayout is not ILayout layout)
				return false;

			for (int i = 0; i < layout.Count; i++)
			{
				var frame = layout[i].Frame;

				// Compare in physical pixels to avoid floating-point rounding between
				// device-independent units and physical pixels. Allow 1-pixel tolerance
				// for sub-pixel alignment differences at various screen densities.
				if ((int)_context.ToPixels(frame.Right) > widthPx + 1
					|| (int)_context.ToPixels(frame.Bottom) > heightPx + 1
					|| (int)_context.ToPixels(frame.Left) < -1
					|| (int)_context.ToPixels(frame.Top) < -1)
				{
					return true;
				}
			}

			return false;
		}

		protected override void OnConfigurationChanged(Configuration? newConfig)
		{
			base.OnConfigurationChanged(newConfig);

			MauiWindowInsetListener.FindListenerForView(this)?.ResetView(this);
			_didSafeAreaEdgeConfigurationChange = true;
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

		WindowInsetsCompat? IHandleWindowInsets.HandleWindowInsets(View view, WindowInsetsCompat insets)
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

			return SafeAreaExtensions.ApplyAdjustedSafeAreaInsetsPx(insets, CrossPlatformLayout, _context, view);
		}

		void IHandleWindowInsets.ResetWindowInsets(View view)
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
