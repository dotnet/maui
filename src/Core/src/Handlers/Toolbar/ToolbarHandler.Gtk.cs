using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, Gtk.Widget>
	{
		protected override Gtk.Widget CreatePlatformElement() => new NotImplementedView(nameof(IToolbar));

		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
		}
	}
}