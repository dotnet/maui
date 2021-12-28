using Tizen.UIExtensions.NUI.GraphicsView;
using Point = Microsoft.Maui.Graphics.Point;

namespace Microsoft.Maui
{
	public partial class WindowOverlay
	{
		SkiaGraphicsView? _graphicsView;

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

			_graphicsView = new SkiaGraphicsView();
			_graphicsView.Drawable = this;
			
			// TODO

			IsNativeViewInitialized = true;
			return IsNativeViewInitialized;
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

			_graphicsView?.Dispose();
			_graphicsView = null;
			IsPlatformViewInitialized = false;
		}

		partial void OnDisableUITouchEventPassthroughSet()
		{
			// TODO
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
