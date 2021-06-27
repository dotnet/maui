using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UIViewController>
	{
		protected override UIViewController CreateNativeElement() => throw new NotImplementedException();

		public static void MapTitle(WindowHandler handler, IWindow window) { }
	}
}