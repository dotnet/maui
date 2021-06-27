using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UI.Xaml.Window>
	{
		protected override UI.Xaml.Window CreateNativeElement() => throw new NotImplementedException();

		public static void MapTitle(WindowHandler handler, IWindow window) { }
	}
}