using System;
using System.Linq;
using Tizen.UIExtensions.NUI.GraphicsView;
using NLayoer = Tizen.NUI.Layer;
using Point = Microsoft.Maui.Graphics.Point;

namespace Microsoft.Maui
{
	public partial class WindowOverlay
	{
		PassthroughView? _graphicsView;
		NLayoer? _overlayLayer;

		public virtual bool Initialize()
		{
			if (IsPlatformViewInitialized)
				return true;

			if (Window == null)
				return false;

			var handler = Window?.Handler as WindowHandler;
			if (handler?.MauiContext == null)
				return false;

			var nativeWindow = handler?.MauiContext.GetPlatformWindow();
			if (nativeWindow == null)
				return false;

			_graphicsView = new PassthroughView(this)
			{
				HeightResizePolicy = Tizen.NUI.ResizePolicyType.FillToParent,
				WidthResizePolicy = Tizen.NUI.ResizePolicyType.FillToParent,
			};
			_graphicsView.Drawable = this;
			_graphicsView.OnTouch += OnTouch;

			_overlayLayer = new NLayoer();
			_overlayLayer.Add(_graphicsView);
			nativeWindow.AddLayer(_overlayLayer);

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

			var nativeWindow = handler?.MauiContext.GetPlatformWindow();
			if (nativeWindow == null)
				return;

			nativeWindow.RemoveLayer(_overlayLayer);

			_overlayLayer?.Remove(_graphicsView);
			_graphicsView?.Dispose();
			_graphicsView = null;
			_overlayLayer?.Dispose();
			_overlayLayer = null;

			IsPlatformViewInitialized = false;
		}

		void OnTouch(object? sender, Point e) =>
			OnTappedInternal(e);
	}

	class PassthroughView : SkiaGraphicsView
	{
		WindowOverlay overlay;

		public PassthroughView(WindowOverlay overlay)
		{
			GrabTouchAfterLeave = true;
			this.overlay = overlay;
			TouchEvent += (s, e) => false;
		}

		public event EventHandler<Point>? OnTouch;

		protected override bool HitTest(Tizen.NUI.Touch touch)
		{
			if (touch == null)
				return false;

			var point = new Point(touch.GetLocalPosition(0).X.ToScaledDP(), touch.GetLocalPosition(0).Y.ToScaledDP());

			var disableTouchEvent = false;

			if (overlay.DisableUITouchEventPassthrough)
				disableTouchEvent = true;
			else if (overlay.EnableDrawableTouchHandling)
				disableTouchEvent = overlay.WindowElements.Any(n => n.Contains(point));

			OnTouch?.Invoke(this, point);
			return disableTouchEvent;
		}
	}
}
