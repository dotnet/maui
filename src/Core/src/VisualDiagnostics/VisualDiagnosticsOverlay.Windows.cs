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
		private HashSet<Tuple<IScrollView, ScrollViewer>> _scrollViews = new HashSet<Tuple<IScrollView, ScrollViewer>>();

		/// <inheritdoc/>
		public IReadOnlyCollection<Tuple<IScrollView, ScrollViewer>> ScrollViews => this._scrollViews.ToList().AsReadOnly();

		/// <inheritdoc/>
		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			var nativeScroll = scrollBar.GetNative(true);
			if (nativeScroll != null && nativeScroll is ScrollViewer viewer)
			{
				if (this._scrollViews.Add(new Tuple<IScrollView, ScrollViewer>(scrollBar, viewer)))
				{
					viewer.ViewChanging += Viewer_ViewChanging;
				}
			}
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
			foreach (var scrollBar in this.ScrollViews)
			{
				scrollBar.Item2.ViewChanging -= Viewer_ViewChanging;
			}

			this._scrollViews.Clear();
		}

		public override void HandleUIChange()
		{
			base.HandleUIChange();
			if (this._windowElements.Any())
				this.RemoveAdorners();
		}

		private void Viewer_ViewChanging(object? sender, ScrollViewerViewChangingEventArgs e)
		{
			this.Invalidate();
		}
	}
}
