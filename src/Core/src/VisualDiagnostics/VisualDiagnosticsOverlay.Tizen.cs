using System;
using System.Collections.Generic;
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
				// TODO
				//if (nativeScroll is Scroller scroller)
				//{
				//	scroller.Scrolled += OnScrolled;
				//}
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
				// TODO
				//if (scroll is Scroller scroller)
				//{
				//	scroller.Scrolled -= OnScrolled;
				//}
			}
			_scrollViews.Clear();
		}

		void OnScrolled(object? sender, EventArgs e)
		{
			Invalidate();
		}
	}
}