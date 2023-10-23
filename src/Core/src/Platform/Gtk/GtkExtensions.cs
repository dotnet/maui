using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{

	public static class GtkExtensions
	{

		public static Size ToSize(this Gtk.Requisition it) =>
			new(it.Width, it.Height);

		public static Gtk.Requisition ToGtkRequisition(this Graphics.Size size) =>
			new()
			{
				Height = (int)size.Height,
				Width = (int)size.Width
			};

	}

}