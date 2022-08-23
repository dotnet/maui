using System;

namespace Microsoft.Maui.Handlers
{
	public partial class BorderHandler : ViewHandler<IBorderView, Gtk.Widget>
	{
		[MissingMapper]
		protected override Gtk.Widget CreatePlatformView() => throw new NotImplementedException();

		[MissingMapper]
		public static void MapContent(IBorderHandler handler, IBorderView border)
		{
		}
	}
}
