using System.Collections.Generic;
using Android.App;
using Android.Views;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui
{
	/// <summary>
	/// Visual Diagnostics Overlay.
	/// </summary>
	public partial class VisualDiagnosticsOverlay
	{
		readonly Dictionary<IScrollView, View> _scrollViews = new();

		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			var nativeScroll = scrollBar.ToPlatform();
			if (nativeScroll != null)
			{
				nativeScroll.ScrollChange += OnScrollChange;
				_scrollViews.Add(scrollBar, nativeScroll);
			}
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
			foreach (var scrollBar in _scrollViews.Values)
			{
				if (!scrollBar.IsDisposed())
					scrollBar.ScrollChange -= OnScrollChange;
			}

			_scrollViews.Clear();
		}

		public override void HandleUIChange()
		{
			base.HandleUIChange();

			if (WindowElements.Count > 0)
				RemoveAdorners();

			if (GraphicsView != null)
				Offset = GenerateAdornerOffset(GraphicsView);
		}

		void OnScrollChange(object? sender, View.ScrollChangeEventArgs e)
		{
			Invalidate();
		}

		/// <summary>
		/// Generates the Adorner Offset.
		/// </summary>
		/// <param name="graphicsView"><see cref="PlatformGraphicsView"/>.</param>
		/// <returns>Offset Rectangle.</returns>
		Point GenerateAdornerOffset(View graphicsView)
		{
			if (graphicsView == null || graphicsView.Context?.GetActivity() is not Activity nativeActivity)
				return new Point();

			if (nativeActivity.Resources == null || nativeActivity.Resources.DisplayMetrics == null)
				return new Point();

			float dpi = nativeActivity.Resources.DisplayMetrics.Density;
			float heightPixels = nativeActivity.Resources.DisplayMetrics.HeightPixels;

			return new Point(0, -(heightPixels - graphicsView.MeasuredHeight) / dpi);
		}
	}
}