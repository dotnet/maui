using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	public partial class WindowOverlay : IWindowOverlay, IDrawable
	{
		internal Activity? _nativeActivity;
		internal NativeGraphicsView? _graphicsView;

		/// <inheritdoc/>
		public bool DisableUITouchEventPassthrough { get; set; }

		public bool InitializeNativeLayer()
		{
			if (this.IsNativeViewInitialized)
				return true;

			if (this.Window == null)
				return false;

			var nativeWindow = this.Window.Content.GetNative(true);
			if (nativeWindow == null)
				return false;
			
			var handler = this.Window.Handler as WindowHandler;
			if (handler == null || handler.MauiContext == null)
				return false;
			var rootManager = handler.MauiContext.GetNavigationRootManager();
			if (rootManager == null)
				return false;


			if (handler.NativeView is not Activity activity)
				return false;

			_nativeActivity = activity;
			var nativeLayer = rootManager.RootView as ViewGroup;

			if (nativeLayer == null || nativeLayer.Context == null)
				return false;

			if (_nativeActivity == null || _nativeActivity.WindowManager == null || _nativeActivity.WindowManager.DefaultDisplay == null)
				return false;

			var measuredHeight = nativeLayer.MeasuredHeight;

			if (_nativeActivity.Window != null)
				_nativeActivity.Window.DecorView.LayoutChange += DecorView_LayoutChange;

			if (_nativeActivity != null && _nativeActivity.Resources != null && _nativeActivity.Resources.DisplayMetrics != null)
				this.DPI = _nativeActivity.Resources.DisplayMetrics.Density;

			this._graphicsView = new NativeGraphicsView(nativeLayer.Context, this);
			if (this._graphicsView == null)
				return false;

			this._graphicsView.Touch += TouchLayer_Touch;
			nativeLayer.AddView(this._graphicsView, 0, new CoordinatorLayout.LayoutParams(CoordinatorLayout.LayoutParams.MatchParent, CoordinatorLayout.LayoutParams.MatchParent));
			this._graphicsView.BringToFront();
			this.IsNativeViewInitialized = true;
			return this.IsNativeViewInitialized;
		}

		/// <inheritdoc/>
		public void Invalidate()
		{
			this._graphicsView?.Invalidate();
		}

		/// <summary>
		/// Disposes the native event hooks and handlers used to drive the overlay.
		/// </summary>
		private void DisposeNativeDependencies()
		{
			if (_nativeActivity?.Window != null)
				_nativeActivity.Window.DecorView.LayoutChange -= DecorView_LayoutChange;
		}

		private void TouchLayer_Touch(object? sender, View.TouchEventArgs e)
		{
			if (e == null || e.Event == null)
				return;

			e.Handled = this.DisableUITouchEventPassthrough;
			var point = new Point(e.Event.RawX, e.Event.RawY);
			OnTouchInternal(point);
		}

		private void DecorView_LayoutChange(object? sender, View.LayoutChangeEventArgs e)
		{
			this.HandleUIChange();
			this.Invalidate();
		}
	}
}
