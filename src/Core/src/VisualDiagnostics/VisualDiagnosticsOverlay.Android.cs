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
	public partial class VisualDiagnosticsOverlay : WindowOverlay
	{
		private HashSet<Tuple<IScrollView, Android.Views.View>> _scrollViews = new HashSet<Tuple<IScrollView, Android.Views.View>>();

		/// <inheritdoc/>
		public IReadOnlyCollection<Tuple<IScrollView, Android.Views.View>> ScrollViews => _scrollViews.ToList().AsReadOnly();

		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			var nativeScroll = scrollBar.GetNative(true);
			if (nativeScroll != null)
			{
				nativeScroll.ScrollChange += ScrollScrollChange;
				_scrollViews.Add(new Tuple<IScrollView, View>(scrollBar, nativeScroll));
			}
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
			foreach (var scrollBar in ScrollViews)
			{
				if (!scrollBar.Item2.IsDisposed())
					scrollBar.Item2.ScrollChange -= ScrollScrollChange;
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

		private void ScrollScrollChange(object? sender, View.ScrollChangeEventArgs e)
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
