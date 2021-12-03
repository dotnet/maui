using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia.Views;
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
			var nativeScroll = scrollBar.GetNative(true);
			if (nativeScroll != null)
			{
				_scrollViews.Add(scrollBar, nativeScroll);
			}
			//TODO : Need to impl
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
			foreach (var scroll in _scrollViews.Values)
			{
				//TODO : Need to impl
			}

			_scrollViews.Clear();
		}
	}
}