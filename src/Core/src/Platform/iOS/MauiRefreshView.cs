﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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
				if (scrollView.ContentOffset.Y < 0)
					return true;

				if (refreshing)
					scrollView.SetContentOffset(new CoreGraphics.CGPoint(0, _originalY - _refreshControlHeight), true);
				else
					scrollView.SetContentOffset(new CoreGraphics.CGPoint(0, _originalY), true);

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

			if (_refreshControl.Superview != null)
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
			_refreshControlParent = view;

			if (view is UIScrollView scrollView)
			{
				if (CanUseRefreshControlProperty())
					scrollView.RefreshControl = _refreshControl;
				else
					scrollView.InsertSubview(_refreshControl, index);

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
			_refreshControl.Enabled = isRefreshViewEnabled;

			UserInteractionEnabled = true;

			if (IsRefreshing)
				return;

			if (isRefreshViewEnabled)
				TryInsertRefresh(_refreshControlParent);
			else
				TryRemoveRefresh(_refreshControlParent);

			UserInteractionEnabled = true;
		}

#pragma warning disable CA1416 // TODO: 'UINavigationBar.PrefersLargeTitles' is only supported on: 'ios' 11.0 and later
		bool CanUseRefreshControlProperty() =>
			this.GetNavigationController()?.NavigationBar?.PrefersLargeTitles ?? true;
#pragma warning restore CA1416
	}
}
