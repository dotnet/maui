using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.CoreGraphics;
using Microsoft.Maui.Graphics.Native;
using UIKit;

namespace Microsoft.Maui
{
	/// <summary>
	/// Visual Diagnostics Overlay.
	/// </summary>
	public partial class VisualDiagnosticsOverlay : WindowOverlay
	{
		private Dictionary<IScrollView, IDisposable> _scrollViews = new Dictionary<IScrollView, IDisposable>();

		/// <inheritdoc/>
		public IReadOnlyDictionary<IScrollView, IDisposable> ScrollViews => this._scrollViews;

		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			var nativeScroll = scrollBar.GetNative(true);
			if (nativeScroll != null)
			{
				var dispose = nativeScroll.AddObserver("contentOffset", Foundation.NSKeyValueObservingOptions.New, FrameAction);
				this._scrollViews.Add(scrollBar, dispose);
			}
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
			foreach (var scroll in this._scrollViews)
			{
				scroll.Value.Dispose();
			}

			this._scrollViews.Clear();
		}

		private void ScrollScrolled(object? sender, EventArgs e)
		{
			this.Invalidate();
		}
		private void FrameAction(Foundation.NSObservedChange obj)
		{
			if (this._windowElements.Any())
				this.RemoveAdorners();

			this.Invalidate();
		}
	}
}
