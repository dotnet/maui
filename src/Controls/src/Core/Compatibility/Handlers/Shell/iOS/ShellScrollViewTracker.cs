using System;
using ObjCRuntime;
using UIKit;
using PointF = CoreGraphics.CGPoint;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class ShellScrollViewTracker : IDisposable, IShellContentInsetObserver
	{
		#region IShellContentInsetObserver

		void IShellContentInsetObserver.OnInsetChanged(Thickness inset, double tabThickness)
		{
			UpdateContentInset(inset, tabThickness);
		}

		#endregion IShellContentInsetObserver

		bool _disposed;
		bool _isInShell;
		bool _isInItems;
		IPlatformViewHandler _renderer;
		UIScrollView _scrollView;
		ShellSection _shellSection;
		IShellSectionController ShellSectionController => _shellSection;

		public ShellScrollViewTracker(IPlatformViewHandler renderer)
		{
			_renderer = renderer;

			if (_renderer.PlatformView is UIScrollView scrollView)
				_scrollView = scrollView;
			else if (_renderer.PlatformView.Subviews.Length > 0 && _renderer.PlatformView.Subviews[0] is UIScrollView nestedScrollView)
				_scrollView = nestedScrollView;

			if (_scrollView == null)
				return;

			var parent = _renderer.VirtualView.Parent;

			while (!Application.IsApplicationOrWindowOrNull(parent))
			{
				if (parent is ScrollView || parent is ListView || parent is TableView || parent is CollectionView)
					break;
				parent = parent.Parent;

				// Currently ShellContents are not pushable onto the stack so we know we are not being pushed
				// on the stack if we are in a ShellContent
				if (parent is ShellContent)
					_isInItems = true;

				if (parent is ShellSection shellSection)
				{
					_isInShell = true;
					_shellSection = shellSection;
					((IShellSectionController)_shellSection).AddContentInsetObserver(this);

					break;
				}
			}

			if (_isInShell)
				UpdateVerticalBounce();
		}

		public void OnLayoutSubviews()
		{
			if (!_isInShell)
				return;

			if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)
#if TVOS
				|| OperatingSystem.IsTvOSVersionAtLeast(11)
#endif
				)
			{
				var newBounds = _scrollView.AdjustedContentInset.InsetRect(_scrollView.Bounds).ToRectangle();
				newBounds.X = 0;
				newBounds.Y = 0;
				if (_renderer.VirtualView is ScrollView scrollView)
					scrollView.LayoutAreaOverride = newBounds;
			}
		}

		Thickness _lastInset;
		double _tabThickness;

		public bool Reset()
		{
			if (!_isInShell)
				return false;

			if (_lastInset == 0 && _tabThickness == 0)
				return false;

			if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11)))
			{
				UpdateContentInset(_lastInset, _tabThickness);
				return true;
			}

			if (ShellSectionController.GetItems().Count > 1 && _isInItems)
			{
				UpdateContentInset(_lastInset, _tabThickness);
				return true;
			}

			return false;
		}

		void UpdateContentInset(Thickness inset, double tabThickness)
		{
			_lastInset = inset;
			_tabThickness = tabThickness;
			if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)
#if TVOS
				|| OperatingSystem.IsTvOSVersionAtLeast(11)
#endif
			)
			{
				if (ShellSectionController.GetItems().Count > 1 && _isInItems)
				{
					var top = (float)tabThickness;

					var delta = _scrollView.ContentInset.Top - top;
					var newInset = new UIEdgeInsets(top, 0, 0, 0);

					if (newInset != _scrollView.ContentInset)
					{
						_scrollView.ContentInset = newInset;

						var currentOffset = _scrollView.ContentOffset;
						_scrollView.ContentOffset = new PointF(currentOffset.X, currentOffset.Y + delta);
					}
				}

				_scrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Always;
			}
			else
			{
				var top = (float)(inset.Top);

				if (_isInItems)
					top += (float)tabThickness;

				var delta = _scrollView.ContentInset.Top - top;
				var newInset = new UIEdgeInsets(top, (float)inset.Left, (float)inset.Bottom, (float)inset.Right);

				if (newInset != _scrollView.ContentInset)
				{
					_scrollView.ContentInset = newInset;

					var currentOffset = _scrollView.ContentOffset;
					_scrollView.ContentOffset = new PointF(currentOffset.X, currentOffset.Y + delta - inset.Bottom);
				}
			}
		}

		void UpdateVerticalBounce()
		{
			// Normally we dont want to do this unless this scrollview is vertical and its
			// element is the child of a Page with a SearchHandler that is collapsible.
			// If we can't bounce in that case you may not be able to expose the handler.
			// Also the hiding behavior only depends on scroll on iOS 11. In 10 and below
			// the search goes in the TitleView so there is nothing to collapse/expand.
			if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11)) ||
				(_renderer.VirtualView is ScrollView scrollView && scrollView.Orientation == ScrollOrientation.Horizontal))
				return;

			var parent = _renderer.VirtualView.Parent;
			while (!Application.IsApplicationOrNull(parent))
			{
				if (parent is Page page)
				{
					var searchHandler = Shell.GetSearchHandler(page);
					if (searchHandler?.SearchBoxVisibility == SearchBoxVisibility.Collapsible)
						_scrollView.AlwaysBounceVertical = true;
					return;
				}
				parent = parent.Parent;
			}
		}

		#region IDisposable Support

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					(_shellSection as IShellSectionController)?.RemoveContentInsetObserver(this);
				}

				_renderer = null;
				_scrollView = null;
				_shellSection = null;

				_disposed = true;
			}
		}

		#endregion IDisposable Support
	}
}