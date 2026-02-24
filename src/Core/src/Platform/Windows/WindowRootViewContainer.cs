using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;

namespace Microsoft.Maui.Platform
{
	internal partial class WindowRootViewContainer : Panel
	{
		FrameworkElement? _topPage;
		UIElementCollection? _cachedChildren;

		[SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "Panel.Children property is banned to enforce use of this CachedChildren property.")]
		internal UIElementCollection CachedChildren
		{
			get
			{
				_cachedChildren ??= Children;
				return _cachedChildren;
			}
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			var width = availableSize.Width;
			var height = availableSize.Height;

			if (double.IsInfinity(width))
				width = XamlRoot.Size.Width;

			if (double.IsInfinity(height))
				height = XamlRoot.Size.Height;

			var size = new Size(width, height);

			// measure the children to fit the container exactly
			foreach (var child in CachedChildren)
			{
				child.Measure(size);
			}

			return size;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			foreach (var child in CachedChildren)
			{
				child.Arrange(new Rect(new Point(0, 0), finalSize));
			}

			return finalSize;
		}

		internal void AddPage(FrameworkElement pageView)
		{
			if (!CachedChildren.Contains(pageView))
			{
				// Disable interaction on the page being covered by the modal
				_topPage?.SetValue(IsHitTestVisibleProperty, false);

				int indexOFTopPage = 0;
				if (_topPage is not null)
					indexOFTopPage = CachedChildren.IndexOf(_topPage) + 1;

				CachedChildren.Insert(indexOFTopPage, pageView);
				_topPage = pageView;

				// Move keyboard focus to the new top page
				TryMoveFocusToPage(_topPage);
			}
		}

		internal void RemovePage(FrameworkElement pageView)
		{
			int indexOFTopPage = -1;
			if (_topPage != null)
				indexOFTopPage = CachedChildren.IndexOf(_topPage) - 1;

			CachedChildren.Remove(pageView);

			if (indexOFTopPage >= 0)
			{
				_topPage = (FrameworkElement)CachedChildren[indexOFTopPage];

				// Re-enable interaction on the revealed page and restore focus
				if (_topPage is not null)
				{
					_topPage.IsHitTestVisible = true;
					TryMoveFocusToPage(_topPage);
				}
			}
			else
			{
				_topPage = null;
			}
		}

		static void TryMoveFocusToPage(FrameworkElement page)
		{
			if (page.IsLoaded)
			{
				SetFocusToFirstElement(page);
			}
			else
			{
				page.Loaded += OnPageLoadedForFocus;
			}
		}

		static void OnPageLoadedForFocus(object sender, RoutedEventArgs e)
		{
			if (sender is FrameworkElement page)
			{
				page.Loaded -= OnPageLoadedForFocus;
				SetFocusToFirstElement(page);
			}
		}

		static void SetFocusToFirstElement(FrameworkElement page)
		{
			if (FocusManager.FindFirstFocusableElement(page) is UIElement focusableElement)
			{
				if (focusableElement.Focus(FocusState.Programmatic))
				{
					return;
				}
			}

			// Fallback: ensure the page itself takes focus so keyboard input does not remain on the underlying content
			page.Focus(FocusState.Programmatic);
		}

		internal void AddOverlay(FrameworkElement overlayView)
		{
			if (!CachedChildren.Contains(overlayView))
				CachedChildren.Add(overlayView);
		}

		internal void RemoveOverlay(FrameworkElement overlayView)
		{
			CachedChildren.Remove(overlayView);
		}
	}
}