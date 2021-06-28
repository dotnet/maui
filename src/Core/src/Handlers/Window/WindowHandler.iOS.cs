using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UIWindow>, INativeWindowHandler
	{
		public static void MapTitle(WindowHandler handler, IWindow window) { }
	}
}