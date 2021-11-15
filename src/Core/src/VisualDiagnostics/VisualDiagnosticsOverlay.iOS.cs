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
		private HashSet<Tuple<IScrollView, IDisposable>> _scrollViews = new HashSet<Tuple<IScrollView, IDisposable>>();

		/// <inheritdoc/>
		public IReadOnlyCollection<Tuple<IScrollView, IDisposable>> ScrollViews => this._scrollViews.ToList().AsReadOnly();

		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			var nativeScroll = scrollBar.GetNative(true);
			if (nativeScroll != null)
			{
				var dispose = nativeScroll.AddObserver("contentOffset", Foundation.NSKeyValueObservingOptions.New, FrameAction);
				this._scrollViews.Add(new Tuple<IScrollView, IDisposable>(scrollBar, dispose));
			}
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
			foreach (var scroll in this._scrollViews)
			{
				scroll.Item2.Dispose();
			}

			this._scrollViews.Clear();
		}

		private void Scroll_Scrolled(object? sender, EventArgs e)
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
