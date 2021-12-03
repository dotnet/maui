using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia.Views;
using Microsoft.Maui.Handlers;
using ElmSharp;
using Point = Microsoft.Maui.Graphics.Point;

namespace Microsoft.Maui
{
	public partial class WindowOverlay
	{
		SkiaGraphicsView? _graphicsView;

		public virtual bool Initialize()
		{
			if (IsNativeViewInitialized)
				return true;

			if (Window == null)
				return false;

			var nativeWindow = Window?.Content?.GetNative(true);
			if (nativeWindow == null)
				return false;

			var handler = Window?.Handler as WindowHandler;
			if (handler?.MauiContext == null)
				return false;

			//TODO: Need to impl
			return false;
		}

		public void Invalidate()
		{
			_graphicsView?.Invalidate();
		}

		void DeinitializeNativeDependencies()
		{
			//TODO: Need to impl

			_graphicsView = null;
			IsNativeViewInitialized = false;
		}
	}
}