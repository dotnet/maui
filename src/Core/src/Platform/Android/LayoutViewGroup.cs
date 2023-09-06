using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Graphics;
using ARect = Android.Graphics.Rect;
using Rectangle = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Platform
{
	public class LayoutViewGroup : ViewGroup, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable, ITouchInterceptingView, IInputTransparentCapable
	{
		readonly ARect _clipRect = new();
		readonly Context _context;

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

			var measure = CrossPlatformMeasure(deviceIndependentWidth, deviceIndependentHeight);

			// If the measure spec was exact, we should return the explicit size value, even if the content
			// measure came out to a different size
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

			var destination = _context.ToCrossPlatformRectInReferenceFrame(l, t, r, b);

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
		}

		WeakReference<IOnTouchListener>? _touchListener;

		public override void SetOnTouchListener(IOnTouchListener? l)
		{
			_touchListener = l is null ? null : new(l);
			base.SetOnTouchListener(l);
		}

		bool ITouchInterceptingView.TouchEventNotReallyHandled { get; set; }

		public override bool OnTouchEvent(MotionEvent? e) =>
			base.OnTouchEvent(e) ||
			TouchEventInterceptor.OnTouchEvent(this, e);

		public override bool DispatchTouchEvent(MotionEvent? e) =>
			TouchEventInterceptor.DispatchingTouchEvent(this, e) &&
			base.DispatchTouchEvent(e) &&
			TouchEventInterceptor.DispatchedTouchEvent(this, e, _touchListener?.GetTargetOrDefault());

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
