using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CoreFoundation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using WebKit;

namespace Microsoft.Maui.Platform
{
	public class MauiRefreshView : MauiView
	{
		bool _isRefreshing;
		bool _isEnabled = true;
		bool _isRefreshEnabled = true;
		nfloat _originalY;
		nfloat _refreshControlHeight;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		UIView _refreshControlParent;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		UIView? _contentView;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		UIRefreshControl _refreshControl;
		public UIRefreshControl RefreshControl => _refreshControl;

		public MauiRefreshView()
		{
			_refreshControl = new UIRefreshControl();
			_refreshControlParent = this;
		}

		public bool IsRefreshing
		{
			get { return _isRefreshing; }
			set
			{
				_isRefreshing = value;

				if (_isRefreshing != _refreshControl.Refreshing)
				{
					if (_isRefreshing)
					{
						TryOffsetRefresh(this, IsRefreshing);
						_refreshControl.BeginRefreshing();
					}
					else
					{
						_refreshControl.EndRefreshing();
						TryOffsetRefresh(this, IsRefreshing);
					}
				}
			}
		}

		public void UpdateContent(IView? content, IMauiContext? mauiContext)
		{
			if (_refreshControlParent is not null)
			{
				TryRemoveRefresh(_refreshControlParent);
			}

			_contentView?.RemoveFromSuperview();

			if (content is not null && mauiContext is not null)
			{
				_contentView = content.ToPlatform(mauiContext);
				AddSubview(_contentView);
				TryInsertRefresh(_contentView);
			}
		}

		bool TryOffsetRefresh(UIView view, bool refreshing)
		{
			if (view is UIScrollView scrollView)
			{
				if (refreshing)
					scrollView.SetContentOffset(new CGPoint(0, _originalY - _refreshControlHeight), true);
				else
					scrollView.ContentOffset = new CGPoint(0, _originalY);

				return true;
			}

			if (view is WKWebView)
			{
				return true;
			}

			if (view.Subviews == null)
				return false;

			for (int i = 0; i < view.Subviews.Length; i++)
			{
				var control = view.Subviews[i];
				if (TryOffsetRefresh(control, refreshing))
					return true;
			}

			return false;
		}

		bool TryRemoveRefresh(UIView view, int index = 0)
		{
			_refreshControlParent = view;
	
			_refreshControl.RemoveFromSuperview();

			if (view is UIScrollView scrollView)
			{
				if (CanUseRefreshControlProperty())
					scrollView.RefreshControl = null;

				return true;
			}

			if (view.Subviews == null)
				return false;

			for (int i = 0; i < view.Subviews.Length; i++)
			{
				var control = view.Subviews[i];
				if (TryRemoveRefresh(control, i))
					return true;
			}

			return false;
		}

		bool TryInsertRefresh(UIView view, int index = 0)
		{
			if (!ShouldAllowRefreshGesture)
			{
				return false;
			}

			_refreshControlParent = view;
 
			if (view is UIScrollView scrollView)
			{
				if (CanUseRefreshControlProperty())
					scrollView.RefreshControl = _refreshControl;
				else
					scrollView.InsertSubview(_refreshControl, index);

				//Setting the bounds so that the refresh control renders above the potential header
				_refreshControl.Bounds = new CGRect(
					_refreshControl.Bounds.X,
					-scrollView.ContentOffset.Y,
					_refreshControl.Bounds.Width,
					_refreshControl.Bounds.Height);

				scrollView.AlwaysBounceVertical = true;

				_originalY = scrollView.ContentOffset.Y;
				_refreshControlHeight = _refreshControl.Frame.Size.Height;

				return true;
			}

			if (view is WKWebView webView)
			{
				webView.ScrollView.InsertSubview(_refreshControl, index);
				return true;
			}

			if (view.Subviews == null)
				return false;

			for (int i = 0; i < view.Subviews.Length; i++)
			{
				var control = view.Subviews[i];
				if (TryInsertRefresh(control, i))
					return true;
			}

			return false;
		}

		public void UpdateIsEnabled(bool isRefreshViewEnabled)
		{
			_isEnabled = isRefreshViewEnabled;
			UpdateRefreshGesture();
		}

		public void UpdateIsRefreshEnabled(bool isRefreshEnabled)
		{
			_isRefreshEnabled = isRefreshEnabled;
			UpdateRefreshGesture();
		}

		void UpdateRefreshGesture()
		{
			if (ShouldAllowRefreshGesture)
			{
				if (!IsRefreshing)
				{
					TryInsertRefresh(_refreshControlParent);
				}
			}
			else
			{
				if (IsRefreshing)
				{
					IsRefreshing = false;
				}
				TryRemoveRefresh(_refreshControlParent);
			}
		}

		bool ShouldAllowRefreshGesture =>
			_isEnabled && _isRefreshEnabled;

		bool CanUseRefreshControlProperty() =>
			this.GetNavigationController()?.NavigationBar?.PrefersLargeTitles ?? true;
	}
}
