using System;
using Android.Content;
using Android.Content.Res;
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
		bool _didSafeAreaEdgeConfigurationChange = true;
		bool _isInsetListenerSet;

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

			// If we're inside a ScrollView, we don't need to set the global listener
			// ScrollViews handle their own insets
			if (Parent is not MauiScrollView)
			{
				_isInsetListenerSet = MauiWindowInsetListenerExtensions.TrySetMauiWindowInsetListener(this, _context);
			}
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			if (_isInsetListenerSet)
				MauiWindowInsetListenerExtensions.RemoveMauiWindowInsetListener(this, _context);
			_didSafeAreaEdgeConfigurationChange = true;
			_isInsetListenerSet = false;
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

			if (_didSafeAreaEdgeConfigurationChange && _isInsetListenerSet)
			{
				ViewCompat.RequestApplyInsets(this);
				_didSafeAreaEdgeConfigurationChange = false;
			}
		}

		protected override void OnConfigurationChanged(Configuration? newConfig)
		{
			base.OnConfigurationChanged(newConfig);

			MauiWindowInsetListener.FindListenerForView(this)?.ResetView(this);
			_didSafeAreaEdgeConfigurationChange = true;
		}

		/// <summary>
		/// Marks that the SafeAreaEdges configuration for this view (or its associated virtual view)
		/// has changed and that window insets should be re-applied on the next layout pass.
		/// </summary>
		internal void MarkSafeAreaEdgeConfigurationChanged()
		{
			_didSafeAreaEdgeConfigurationChange = true;
			// Ensure a layout pass so that OnLayout will trigger InvalidateWindowInsets
			RequestLayout();
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
			System.Diagnostics.Debug.WriteLine($"MAUI_INSETS: ResetWindowInsets called on {GetType().Name}");
			System.Diagnostics.Debug.WriteLine($"MAUI_INSETS: Current padding: L={PaddingLeft} T={PaddingTop} R={PaddingRight} B={PaddingBottom}");
			System.Diagnostics.Debug.WriteLine($"MAUI_INSETS: _hasStoredOriginalPadding={_hasStoredOriginalPadding}");
			
			if (_hasStoredOriginalPadding)
			{
				System.Diagnostics.Debug.WriteLine($"MAUI_INSETS: Restoring original padding: L={_originalPadding.left} T={_originalPadding.top} R={_originalPadding.right} B={_originalPadding.bottom}");
				SetPadding(_originalPadding.left, _originalPadding.top, _originalPadding.right, _originalPadding.bottom);
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"MAUI_INSETS: No original padding stored, keeping current padding");
			}
		}

		#endregion
	}
}