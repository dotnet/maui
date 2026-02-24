using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
				int indexOFTopPage = 0;
				if (_topPage != null)
					indexOFTopPage = CachedChildren.IndexOf(_topPage) + 1;

				CachedChildren.Insert(indexOFTopPage, pageView);
				_topPage = pageView;
			}
		}

		internal void RemovePage(FrameworkElement pageView)
		{
			int indexOFTopPage = -1;
			if (_topPage != null)
				indexOFTopPage = CachedChildren.IndexOf(_topPage) - 1;

			CachedChildren.Remove(pageView);

			if (indexOFTopPage >= 0)
				_topPage = (FrameworkElement)CachedChildren[indexOFTopPage];
			else
				_topPage = null;
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