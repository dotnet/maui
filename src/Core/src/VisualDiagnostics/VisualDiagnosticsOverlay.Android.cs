using System;
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
			if (nativeScroll != null && OperatingSystem.IsAndroidVersionAtLeast(23))
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
				if (!scrollBar.IsDisposed() && OperatingSystem.IsAndroidVersionAtLeast(23))
					scrollBar.ScrollChange -= OnScrollChange;
			}

			_scrollViews.Clear();
		}

		public override void HandleUIChange()
		{
			base.HandleUIChange();

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

			var decorView = nativeActivity.Window?.DecorView;
			var rectangle = new Android.Graphics.Rect();

			if (decorView is not null)
			{
				decorView.GetWindowVisibleDisplayFrame(rectangle);
			}

			float dpi = nativeActivity.Resources.DisplayMetrics.Density;
			return new Point(0, -(rectangle.Top / dpi));
		}
	}
}