using ElmSharp;
using Microsoft.Maui.Graphics.Skia.Views;
using Point = Microsoft.Maui.Graphics.Point;

namespace Microsoft.Maui
{
	public partial class WindowOverlay
	{
		SkiaGraphicsView? _graphicsView;
		GestureLayer? _touchLayer;

		public virtual bool Initialize()
		{
			if (IsNativeViewInitialized)
				return true;

			if (Window == null)
				return false;

			var nativeWindow = Window.Content?.GetNative(true);
			if (nativeWindow == null)
				return false;

			var handler = Window.Handler as WindowHandler;
			if (handler?.MauiContext == null)
				return false;

			_graphicsView = new SkiaGraphicsView(handler.MauiContext.Context!.BaseLayout);
			_graphicsView.Drawable = this;
			_graphicsView.RepeatEvents = !DisableUITouchEventPassthrough;

			_touchLayer = new GestureLayer(handler.MauiContext.Context!.BaseLayout);
			_touchLayer.Attach(_graphicsView);
			_touchLayer.SetTapCallback(GestureLayer.GestureType.Tap, GestureLayer.GestureState.Start, (data) =>
			{
				var x = _touchLayer.EvasCanvas.Pointer.X;
				var y = _touchLayer.EvasCanvas.Pointer.Y;
				OnTappedInternal(new Point(DPExtensions.ConvertToScaledDP(x), DPExtensions.ConvertToScaledDP(y)));
			});

			handler.MauiContext.Context.SetOverlay(_graphicsView);
			IsNativeViewInitialized = true;
			return IsNativeViewInitialized;
		}

		public void Invalidate()
		{
			_graphicsView?.Invalidate();
		}

		void DeinitializeNativeDependencies()
		{
			if (Window == null)
				return;

			var nativeWindow = Window?.Content?.GetNative(true);
			if (nativeWindow == null)
				return;

			var handler = Window?.Handler as WindowHandler;
			if (handler?.MauiContext == null)
				return;

			_graphicsView?.Unrealize();
			_graphicsView = null;
			IsNativeViewInitialized = false;
		}

		partial void OnDisableUITouchEventPassthroughSet()
		{
			if (_graphicsView != null)
			{
				_graphicsView.RepeatEvents = !DisableUITouchEventPassthrough;
			}
		}
	}
}