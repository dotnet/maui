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
		public IReadOnlyCollection<Tuple<IScrollView, Android.Views.View>> ScrollViews => this._scrollViews.ToList().AsReadOnly();

		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			var nativeScroll = scrollBar.GetNative(true);
			if (nativeScroll != null)
			{
				nativeScroll.ScrollChange += Scroll_ScrollChange;
				this._scrollViews.Add(new Tuple<IScrollView, View>(scrollBar, nativeScroll));
			}
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
			foreach (var scrollBar in this.ScrollViews)
			{
				if (!scrollBar.Item2.IsDisposed())
					scrollBar.Item2.ScrollChange -= Scroll_ScrollChange;
			}

			this._scrollViews.Clear();
		}

		public override void HandleUIChange()
		{
			base.HandleUIChange();
			if (this._drawables.Any())
				this.RemoveAdorners();

			if (this._graphicsView != null && this._nativeActivity != null)
				this.Offset = GenerateAdornerOffset(this._nativeActivity, this._graphicsView);
		}

		private void Scroll_ScrollChange(object? sender, View.ScrollChangeEventArgs e)
		{
			this.Invalidate();
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
