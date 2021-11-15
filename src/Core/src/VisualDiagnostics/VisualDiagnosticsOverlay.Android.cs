using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native;

namespace Microsoft.Maui
{
	/// <summary>
	/// Visual Diagnostics Overlay.
	/// </summary>
	public partial class VisualDiagnosticsOverlay : WindowOverlay, IVisualDiagnosticsOverlay
	{
		private Dictionary<IScrollView, Android.Views.View> _scrollViews = new Dictionary<IScrollView, Android.Views.View>();

		/// <inheritdoc/>
		public IReadOnlyDictionary<IScrollView, Android.Views.View> ScrollViews => _scrollViews;

		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			var nativeScroll = scrollBar.GetNative(true);
			if (nativeScroll != null)
			{
				nativeScroll.ScrollChange += ScrollChange;
				_scrollViews.Add(scrollBar, nativeScroll);
			}
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
			foreach (var scrollBar in ScrollViews.Values)
			{
				if (!scrollBar.IsDisposed())
					scrollBar.ScrollChange -= ScrollChange;
			}

			_scrollViews.Clear();
		}

		public override void HandleUIChange()
		{
			base.HandleUIChange();
			if (_windowElements.Any())
				RemoveAdorners();

			if (_graphicsView != null && _nativeActivity != null)
				Offset = GenerateAdornerOffset(_nativeActivity, _graphicsView);
		}

		private void ScrollChange(object? sender, View.ScrollChangeEventArgs e)
		{
			Invalidate();
		}

		/// <summary>
		/// Generates the Adorner Offset.
		/// </summary>
		/// <param name="nativeActivity">Android Activity, <see cref="Activity"/>.</param>
		/// <param name="graphicsView"><see cref="NativeGraphicsView"/>.</param>
		/// <returns>Offset Rectangle.</returns>
		private Point GenerateAdornerOffset(Activity nativeActivity, NativeGraphicsView graphicsView)
		{
			if (nativeActivity.Resources == null || nativeActivity.Resources.DisplayMetrics == null)
				return new Point();

			if (graphicsView == null)
				return new Point();

			float dpi = nativeActivity.Resources.DisplayMetrics.Density;
			float heightPixels = nativeActivity.Resources.DisplayMetrics.HeightPixels;

			return new Point(0, -(heightPixels - graphicsView.MeasuredHeight) / dpi);
		}
	}
}
