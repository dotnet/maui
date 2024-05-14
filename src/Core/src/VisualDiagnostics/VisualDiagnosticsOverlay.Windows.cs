using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	/// <summary>
	/// Visual Diagnostics Overlay.
	/// </summary>
	public partial class VisualDiagnosticsOverlay
	{
		readonly Dictionary<IScrollView, ScrollViewer> _scrollViews = new();

		/// <inheritdoc/>
		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			if (scrollBar == null)
				return;

			var nativeScroll = scrollBar.ToPlatform();

			if (nativeScroll != null && nativeScroll is ScrollViewer viewer)
			{
				if (_scrollViews.TryAdd(scrollBar, viewer))
				{
					viewer.ViewChanging += OnViewChanging;
				}
			}
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
			foreach (var scrollBar in _scrollViews.Values)
			{
				scrollBar.ViewChanging -= OnViewChanging;
			}

			_scrollViews.Clear();
		}

		public override void HandleUIChange()
		{
			base.HandleUIChange();

			Invalidate();
		}

		void OnViewChanging(object? sender, ScrollViewerViewChangingEventArgs e)
		{
			Invalidate();
		}
	}
}