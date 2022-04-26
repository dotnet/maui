using System;
using System.Collections.Generic;
using ElmSharp;

namespace Microsoft.Maui
{
	/// <summary>
	/// Visual Diagnostics Overlay.
	/// </summary>
	public partial class VisualDiagnosticsOverlay
	{
		readonly Dictionary<IScrollView, EvasObject> _scrollViews = new();

		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			var nativeScroll = scrollBar.ToPlatform();
			if (nativeScroll != null)
			{
				_scrollViews.Add(scrollBar, nativeScroll);
				if (nativeScroll is Scroller scroller)
				{
					scroller.Scrolled += OnScrolled;
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
				if (scroll is Scroller scroller)
				{
					scroller.Scrolled -= OnScrolled;
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