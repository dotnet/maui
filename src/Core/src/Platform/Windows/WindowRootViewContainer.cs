using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace Microsoft.Maui.Platform
{
	internal partial class WindowRootViewContainer : Panel
	{
		FrameworkElement? _topPage;
		UIElementCollection? _cachedChildren;
		bool _modalFocusTrapActive;

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
				if (_topPage is not null)
				{
					// Block pointer/touch input on the page being covered
					_topPage.IsHitTestVisible = false;
				}

				int indexOFTopPage = 0;
				if (_topPage is not null)
					indexOFTopPage = CachedChildren.IndexOf(_topPage) + 1;

				CachedChildren.Insert(indexOFTopPage, pageView);
				_topPage = pageView;

				// When covering another page, activate keyboard focus trapping
				// so Tab cannot escape the modal — mirroring how WinUI ContentDialog works.
				if (indexOFTopPage > 0)
				{
					EnableModalFocusTrap();
				}

				TryMoveFocusToPage(_topPage);
			}
		}

		internal void RemovePage(FrameworkElement pageView)
		{
			// Clean up any pending Loaded handler to prevent memory leaks
			pageView.Loaded -= OnPageLoadedForFocus;

			CachedChildren.Remove(pageView);

			if (CachedChildren.Count > 0)
			{
				_topPage = (FrameworkElement)CachedChildren[CachedChildren.Count - 1];

				// Re-enable pointer/touch on the revealed page
				_topPage.IsHitTestVisible = true;

				// If only one page remains, no focus trapping is needed
				if (CachedChildren.Count <= 1)
				{
					DisableModalFocusTrap();
				}

				TryMoveFocusToPage(_topPage);
			}
			else
			{
				_topPage = null;
				DisableModalFocusTrap();
			}
		}

		void EnableModalFocusTrap()
		{
			if (!_modalFocusTrapActive)
			{
				// Use AddHandler with handledEventsToo so we intercept focus changes
				// even if a child element already handled the event.
				AddHandler(GettingFocusEvent, new TypedEventHandler<UIElement, GettingFocusEventArgs>(OnContainerGettingFocus), true);
				_modalFocusTrapActive = true;
			}
		}

		void DisableModalFocusTrap()
		{
			if (_modalFocusTrapActive)
			{
				RemoveHandler(GettingFocusEvent, new TypedEventHandler<UIElement, GettingFocusEventArgs>(OnContainerGettingFocus));
				_modalFocusTrapActive = false;
			}
		}

		void OnContainerGettingFocus(UIElement sender, GettingFocusEventArgs args)
		{
			if (_topPage is null || args.NewFocusedElement is not DependencyObject newElement)
				return;

			// Allow focus changes within the current top (modal) page
			if (newElement == _topPage || IsDescendantOf(newElement, _topPage))
				return;

			// Focus is trying to leave the modal — redirect it back
			if (FocusManager.FindFirstFocusableElement(_topPage) is DependencyObject firstFocusable)
			{
				args.TrySetNewFocusedElement(firstFocusable);
			}
			else
			{
				args.TryCancel();
			}
		}

		static bool IsDescendantOf(DependencyObject element, DependencyObject ancestor)
		{
			var current = VisualTreeHelper.GetParent(element);
			while (current is not null)
			{
				if (current == ancestor)
					return true;
				current = VisualTreeHelper.GetParent(current);
			}
			return false;
		}

		static void TryMoveFocusToPage(FrameworkElement page)
		{
			if (page.IsLoaded)
			{
				SetFocusToFirstElement(page);
			}
			else
			{
				page.Loaded -= OnPageLoadedForFocus;
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
					return;
			}

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