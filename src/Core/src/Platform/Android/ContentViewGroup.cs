using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Rectangle = Microsoft.Maui.Graphics.Rect;

namespace Microsoft.Maui.Platform
{
	public class ContentViewGroup : PlatformContentViewGroup, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable, IHandleWindowInsets
	{
		IBorderStroke? _clip;
		readonly Context _context;

		public ContentViewGroup(Context context) : base(context)
		{
			_context = context;
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

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (CrossPlatformLayout is null)
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

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			if (CrossPlatformLayout is null)
			{
				return;
			}

			var destination = _context.ToCrossPlatformRectInReferenceFrame(left, top, right, bottom);

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

		#region IHandleWindowInsets Implementation

		private (int left, int top, int right, int bottom) _originalPadding;
		private bool _hasStoredOriginalPadding;


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

			if (IsScrollViewDescendant())
			{
				// For ScrollView descendants, consume insets without applying padding
				// This prevents double application of safe area insets
				return WindowInsetsCompat.Consumed;
			}

			if (!_hasStoredOriginalPadding)
			{
				_originalPadding = (PaddingLeft, PaddingTop, PaddingRight, PaddingBottom);
				_hasStoredOriginalPadding = true;
			}

			var processedInsets = SafeAreaExtensions.GetAdjustedSafeAreaInsets(insets, CrossPlatformLayout, _context);

			// Apply all insets to content view group
			SetPadding((int)_context.ToPixels(processedInsets.Left), (int)_context.ToPixels(processedInsets.Top), (int)_context.ToPixels(processedInsets.Right), (int)_context.ToPixels(processedInsets.Bottom));

			if (processedInsets.Top > 0 || processedInsets.Bottom > 0 || processedInsets.Left > 0 || processedInsets.Right > 0)
			{
				// Consume all insets since we handled them
				return WindowInsetsCompat.Consumed;
			}

			return insets;
		}

		public void ResetWindowInsets(View view)
		{
			// Check if this ContentViewGroup is a descendant of a ScrollView
			if (IsScrollViewDescendant())
			{
				// For ScrollView descendants, don't reset padding since we didn't apply any
				return;
			}

			if (_hasStoredOriginalPadding)
			{
				SetPadding(_originalPadding.left, _originalPadding.top, _originalPadding.right, _originalPadding.bottom);
			}
		}

		#endregion

		bool IsScrollViewDescendant()
		{
			// Check if this ContentViewGroup is a descendant of a ScrollView
			return this.FindParent(parent => parent is MauiScrollView) is not null;
		}
	}
}