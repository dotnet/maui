using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UIWindow>, INativeWindowHandler
	{
		UIWindow? _window;

		protected override UIWindow CreateNativeElement() =>
			_window ?? throw new InvalidOperationException("iOS does now support creating new windows directly.");

		public static void MapTitle(WindowHandler handler, IWindow window) { }

		public void SetWindow(UIWindow window) =>
			_window = window;
	}
}