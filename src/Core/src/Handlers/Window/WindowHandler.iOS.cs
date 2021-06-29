using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UIWindow>
	{
		public static void MapTitle(WindowHandler handler, IWindow window) { }
	}
}