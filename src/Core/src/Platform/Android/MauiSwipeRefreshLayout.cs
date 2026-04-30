using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Platform
{
	public class MauiSwipeRefreshLayout : SwipeRefreshLayout
	{
		readonly Context _context;
		AView? _contentView;
		bool _refreshEnabled = true;
		AWebView? _activeTouchWebView;
		bool _webViewOwnsGesture;
		bool _touchStartedInWebView;

		public MauiSwipeRefreshLayout(Context context) : base(context)
		{
			_context = context;

			// This works around a bug in SwipeRefreshLayout
			// https://github.com/dotnet/maui/pull/17647#discussion_r1433358418
			// https://issuetracker.google.com/issues/110463864
			// It looks like this issue is fixed on the main branch of Android but it hasn't made its way into the packages yet
			SetProgressViewOffset(true, ProgressViewStartOffset, ProgressViewEndOffset - Math.Abs(ProgressViewStartOffset));
		}

		public ICrossPlatformLayout? CrossPlatformLayout
		{
			get;
			set;
		}

		public override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			// Always call base.OnMeasure — unlike ContentViewGroup/LayoutViewGroup,
			// SwipeRefreshLayout.onMeasure internally measures its spinner indicator
			// (mCircleView). Skipping this leaves the spinner at 0x0, making it invisible.
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

			if (CrossPlatformLayout is null)
			{
				return;
			}

			var deviceIndependentWidth = widthMeasureSpec.ToDouble(_context);
			var deviceIndependentHeight = heightMeasureSpec.ToDouble(_context);

			var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
			var heightMode = MeasureSpec.GetMode(heightMeasureSpec);

			var measure = CrossPlatformLayout.CrossPlatformMeasure(deviceIndependentWidth, deviceIndependentHeight);

			// Unlike ContentViewGroup/LayoutViewGroup, SwipeRefreshLayout internally positions
			// its spinner at getMeasuredWidth()/2. We must use the full spec size for both
			// Exactly and AtMost modes (matching View.getDefaultSize behavior) so the spinner
			// is centered correctly. Only for Unspecified do we use the cross-platform measure.
			var width = widthMode == MeasureSpecMode.Unspecified ? measure.Width : deviceIndependentWidth;
			var height = heightMode == MeasureSpecMode.Unspecified ? measure.Height : deviceIndependentHeight;

			var platformWidth = _context.ToPixels(width);
			var platformHeight = _context.ToPixels(height);

			// Minimum values win over everything
			platformWidth = Math.Max(MinimumWidth, platformWidth);
			platformHeight = Math.Max(MinimumHeight, platformHeight);

			SetMeasuredDimension((int)platformWidth, (int)platformHeight);
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			// Always call base.OnLayout — SwipeRefreshLayout.onLayout positions the
			// spinner indicator (mCircleView) centered horizontally. Without this,
			// the spinner won't appear or will be mispositioned.
			base.OnLayout(changed, left, top, right, bottom);
			if (CrossPlatformLayout is null)
			{
				return;
			}

			var destination = _context.ToCrossPlatformRectInReferenceFrame(left, top, right, bottom);
			CrossPlatformLayout?.CrossPlatformArrange(destination);
		}

		public bool RefreshEnabled
		{
			get => _refreshEnabled;
			set => _refreshEnabled = value;
		}

		public void UpdateContent(IView? content, IMauiContext? mauiContext)
		{
			_contentView?.RemoveFromParent();

			if (content != null && mauiContext != null)
			{
				_contentView = content.ToPlatform(mauiContext);
				var layoutParams = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
				AddView(_contentView, layoutParams);
			}
		}

		internal ImageView? CircleImageView
		{
			get
			{
				for (int i = 0; i < ChildCount; i++)
				{
					var child = GetChildAt(i);

					if (child is ImageView iv && child != _contentView)
						return iv;
				}

				return null;
			}
		}

		public override bool CanChildScrollUp()
		{
			// When refresh is disabled, always return true to prevent pull-to-refresh
			if (!_refreshEnabled)
				return true;

			if (ChildCount == 0)
				return base.CanChildScrollUp();

			return CanScrollUp(_contentView);
		}

		public override bool OnInterceptTouchEvent(MotionEvent? ev)
		{
			if (ev is null)
				return false;

			switch (ev.ActionMasked)
			{
				case MotionEventActions.Down:
					_activeTouchWebView = FindWebView(_contentView, ev.GetX(), ev.GetY());
					_touchStartedInWebView = _activeTouchWebView is not null;
					_webViewOwnsGesture = _touchStartedInWebView &&
						RefreshViewWebViewScrollCapture.TryGetCanScrollUp(_activeTouchWebView, out var canScrollUpAtStart) &&
						canScrollUpAtStart;
					if (_webViewOwnsGesture)
					{
						// Forward to base so SwipeRefreshLayout records the initial pointer ID
						// and Y position – required for correct mid-gesture intercept if the
						// web content scrolls to the top during the same drag.
						base.OnInterceptTouchEvent(ev);
						return false;
					}
					break;
				case MotionEventActions.Move:
					// Re-evaluate scrollability so that once the WebView reaches the top,
					// RefreshLayout can start intercepting mid-gesture.
					if (_touchStartedInWebView && _webViewOwnsGesture && _activeTouchWebView is not null)
					{
						if (!RefreshViewWebViewScrollCapture.TryGetCanScrollUp(_activeTouchWebView, out var canStillScrollUp) || !canStillScrollUp)
						{
							_webViewOwnsGesture = false;
						}
					}
					if (_touchStartedInWebView && _webViewOwnsGesture)
						return false;
					break;
				case MotionEventActions.Cancel:
				case MotionEventActions.Up:
					_activeTouchWebView = null;
					_touchStartedInWebView = false;
					_webViewOwnsGesture = false;
					break;
			}

			return base.OnInterceptTouchEvent(ev);
		}

		bool CanScrollUp(AView? view)
		{
			if (!(view is ViewGroup viewGroup))
				return base.CanChildScrollUp();

			if (!CanScrollUpViewByType(view))
				return false;

			for (int i = 0; i < viewGroup.ChildCount; i++)
			{
				var child = viewGroup.GetChildAt(i);

				if (!CanScrollUpViewByType(child))
					return false;

				if (child is SwipeRefreshLayout)
				{
					return CanScrollUp(child as ViewGroup);
				}
			}

			return true;
		}

		static bool CanScrollUpViewByType(AView? view)
		{
			if (view is AbsListView absListView)
			{
				if (absListView.FirstVisiblePosition == 0)
				{
					var subChild = absListView.GetChildAt(0);

					return subChild != null && subChild.Top != 0;
				}

				return true;
			}

			if (view is RecyclerView recyclerView)
				return recyclerView.ComputeVerticalScrollOffset() > 0;

#pragma warning disable XAOBS001 // Obsolete
			if (view is NestedScrollView nestedScrollView)
				return nestedScrollView.ComputeVerticalScrollOffset() > 0;
#pragma warning restore XAOBS001 // Obsolete

			if (view is AWebView webView)
				return RefreshViewWebViewScrollCapture.TryGetCanScrollUp(webView, out var canScrollUp) && canScrollUp;

			return true;
		}

		// Recursively hit-tests the view tree to find a WebView at the given
		// coordinates (in the parent's coordinate space).
		// ScrollX/ScrollY are added when converting to a child's local coordinate
		// space so that scrolled containers (HorizontalScrollView, NestedScrollView,
		// etc.) are handled correctly. Without this adjustment, any ViewGroup that
		// has been scrolled would cause the hit-test to miss the WebView or match
		// the wrong region.
		static AWebView? FindWebView(AView? view, float x, float y)
		{
			if (view is null || view.Visibility != ViewStates.Visible)
				return null;

			if (x < view.Left || x > view.Right || y < view.Top || y > view.Bottom)
				return null;

			if (view is AWebView)
				return (AWebView)view;

			if (view is not ViewGroup viewGroup)
				return null;

			var localX = x - view.Left + view.ScrollX;
			var localY = y - view.Top  + view.ScrollY;

			for (int i = viewGroup.ChildCount - 1; i >= 0; i--)
			{
				var webView = FindWebView(viewGroup.GetChildAt(i), localX, localY);
				if (webView is not null)
					return webView;
			}

			return null;
		}

	}
}
