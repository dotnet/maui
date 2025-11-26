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
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

			if (CrossPlatformLayout is null)
			{
				return;
			}

			var deviceIndependentWidth = widthMeasureSpec.ToDouble(_context);
			var deviceIndependentHeight = heightMeasureSpec.ToDouble(_context);

			CrossPlatformLayout?.CrossPlatformMeasure(deviceIndependentWidth, deviceIndependentHeight);
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
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
				return webView.ScrollY > 0;

			return true;
		}
	}
}
