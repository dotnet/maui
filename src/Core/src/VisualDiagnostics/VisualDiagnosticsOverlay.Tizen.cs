using System;
using System.Collections.Generic;
using Tizen.NUI.Components;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui
{
	/// <summary>
	/// Visual Diagnostics Overlay.
	/// </summary>
	public partial class VisualDiagnosticsOverlay
	{
		readonly Dictionary<IScrollView, NView> _scrollViews = new();

		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			var nativeScroll = scrollBar.ToPlatform();
			if (nativeScroll != null)
			{
				_scrollViews.Add(scrollBar, nativeScroll);
				if (nativeScroll is ScrollableBase scroller)
				{
					scroller.Scrolling -= OnScrolled;
				}
			}
		}

		public override void HandleUIChange()
		{
			base.HandleUIChange();

			if (WindowElements.Count > 0)
				RemoveAdorners();

			Invalidate();
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
			foreach (var scroll in _scrollViews.Values)
			{
				if (scroll is ScrollableBase scroller)
				{
					scroller.Scrolling -= OnScrolled;
				}
			}
			_scrollViews.Clear();
		}

		void OnScrolled(object? sender, EventArgs e)
		{
			Invalidate();
		}
	}
}