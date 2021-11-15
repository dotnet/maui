using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	/// <summary>
	/// Visual Diagnostics Overlay.
	/// </summary>
	public partial class VisualDiagnosticsOverlay : WindowOverlay
	{
		private Dictionary<IScrollView, ScrollViewer> _scrollViews = new Dictionary<IScrollView, ScrollViewer>();

		/// <inheritdoc/>
		public IReadOnlyDictionary<IScrollView, ScrollViewer> ScrollViews => this._scrollViews;

		/// <inheritdoc/>
		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			var nativeScroll = scrollBar.GetNative(true);
			if (nativeScroll != null && nativeScroll is ScrollViewer viewer)
			{
				if (!this._scrollViews.ContainsKey(scrollBar))
				{
					this._scrollViews.Add(scrollBar, viewer);
					viewer.ViewChanging += ViewerViewChanging;
				}
			}
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
			foreach (var scrollBar in this.ScrollViews.Values)
			{
				scrollBar.ViewChanging -= ViewerViewChanging;
			}

			this._scrollViews.Clear();
		}

		public override void HandleUIChange()
		{
			base.HandleUIChange();
			if (this._windowElements.Any())
				this.RemoveAdorners();
		}

		private void ViewerViewChanging(object? sender, ScrollViewerViewChangingEventArgs e)
		{
			this.Invalidate();
		}
	}
}
