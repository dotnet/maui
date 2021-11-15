using System;
using System.Collections.Generic;
using System.Linq;
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
		ViewGroup? _nativeLayer;

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
			_nativeLayer = rootManager.RootView as ViewGroup;

			if (_nativeLayer == null || _nativeLayer.Context == null)
				return false;

			if (_nativeActivity == null || _nativeActivity.WindowManager == null || _nativeActivity.WindowManager.DefaultDisplay == null)
				return false;

			var measuredHeight = _nativeLayer.MeasuredHeight;

			if (_nativeActivity.Window != null)
				_nativeActivity.Window.DecorView.LayoutChange += DecorView_LayoutChange;

			if (_nativeActivity != null && _nativeActivity.Resources != null && _nativeActivity.Resources.DisplayMetrics != null)
				this.DPI = _nativeActivity.Resources.DisplayMetrics.Density;

			this._graphicsView = new NativeGraphicsView(_nativeLayer.Context, this);
			if (this._graphicsView == null)
				return false;

			this._graphicsView.Touch += TouchLayer_Touch;
			_nativeLayer.AddView(this._graphicsView, 0, new CoordinatorLayout.LayoutParams(CoordinatorLayout.LayoutParams.MatchParent, CoordinatorLayout.LayoutParams.MatchParent));
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

			if (_nativeLayer != null)
				_nativeLayer.RemoveView(this._graphicsView);

			this._graphicsView = null;
			this.IsNativeViewInitialized = false;
		}

		private void TouchLayer_Touch(object? sender, View.TouchEventArgs e)
		{
			if (e == null || e.Event == null)
				return;

			System.Diagnostics.Debug.WriteLine(e);
			if (e.Event.Action != MotionEventActions.Down && e.Event.ButtonState != MotionEventButtonState.Primary)
				return;

			var point = new Point(e.Event.RawX, e.Event.RawY);
			
			if (this.DisableUITouchEventPassthrough)
				e.Handled = true;
			else if (this.EnableDrawableTouchHandling)
				e.Handled = this._windowElements.Any(n => n.IsPointInElement(point));

			OnTouchInternal(point);
		}

		private void DecorView_LayoutChange(object? sender, View.LayoutChangeEventArgs e)
		{
			this.HandleUIChange();
			this.Invalidate();
		}
	}
}
