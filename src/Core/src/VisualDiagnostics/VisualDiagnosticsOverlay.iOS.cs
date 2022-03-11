using System;
using System.Collections.Generic;
using Foundation;

namespace Microsoft.Maui
{
	/// <summary>
	/// Visual Diagnostics Overlay.
	/// </summary>
	public partial class VisualDiagnosticsOverlay
	{
		const string ScrollViewContentOffsetKey = "contentOffset";

		readonly Dictionary<IScrollView, IDisposable> _scrollViews = new();

		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			var nativeScroll = scrollBar.ToPlatform();
			if (nativeScroll != null)
			{
				var dispose = nativeScroll.AddObserver(ScrollViewContentOffsetKey, NSKeyValueObservingOptions.New, FrameAction);
				_scrollViews.Add(scrollBar, dispose);
			}
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
			foreach (var scroll in _scrollViews.Values)
			{
				scroll.Dispose();
			}

			_scrollViews.Clear();
		}

		void ScrollScrolled(object? sender, EventArgs e)
		{
			Invalidate();
		}

		void FrameAction(Foundation.NSObservedChange obj)
		{
			Invalidate();
		}
	}
}