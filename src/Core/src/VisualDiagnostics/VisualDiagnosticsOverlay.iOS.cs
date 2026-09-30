using System;
using System.Collections.Generic;
using UIKit;

namespace Microsoft.Maui
{
	/// <summary>
	/// Visual Diagnostics Overlay.
	/// </summary>
	public partial class VisualDiagnosticsOverlay
	{
		readonly Dictionary<IScrollView, IDisposable> _scrollViews = new();

		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			var nativeScroll = scrollBar.ToPlatform();
			if (nativeScroll is UIScrollView uiScrollView)
			{
				var dispose = KeyValueObservation.ObserveContentOffset(uiScrollView, FrameAction);
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

		void FrameAction()
		{
			Invalidate();
		}
	}
}