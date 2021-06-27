using System;
using Android.App;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Activity>
	{
		protected override Activity CreateNativeElement() => throw new NotImplementedException();

		public static void MapTitle(WindowHandler handler, IWindow window) { }
	}
}