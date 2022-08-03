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
			if (IsPlatformViewInitialized)
				return true;

			if (Window == null)
				return false;

			var platformWindow = Window.Content?.ToPlatform() as Window;
			if (platformWindow == null)
				return false;

			var handler = Window.Handler as WindowHandler;
			if (handler?.MauiContext == null)
				return false;

			_graphicsView = new SkiaGraphicsView(platformWindow);
			_graphicsView.Drawable = this;
			_graphicsView.RepeatEvents = !DisableUITouchEventPassthrough;

			_touchLayer = new GestureLayer(platformWindow);
			_touchLayer.Attach(_graphicsView);
			_touchLayer.SetTapCallback(GestureLayer.GestureType.Tap, GestureLayer.GestureState.Start, (data) =>
			{
				var x = _touchLayer.EvasCanvas.Pointer.X;
				var y = _touchLayer.EvasCanvas.Pointer.Y;
				OnTappedInternal(new Point(DPExtensions.ConvertToScaledDP(x), DPExtensions.ConvertToScaledDP(y)));
			});

			platformWindow.SetOverlay(_graphicsView);
			IsPlatformViewInitialized = true;
			return IsPlatformViewInitialized;
		}

		public void Invalidate()
		{
			_graphicsView?.Invalidate();
		}

		void DeinitializePlatformDependencies()
		{
			if (Window == null)
				return;

			var platformWindow = Window?.Content?.ToPlatform();
			if (platformWindow == null)
				return;

			var handler = Window?.Handler as WindowHandler;
			if (handler?.MauiContext == null)
				return;

			_graphicsView?.Unrealize();
			_graphicsView = null;
			IsPlatformViewInitialized = false;
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