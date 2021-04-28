namespace Microsoft.Maui.Graphics.Native.Gtk
{

	public static class GraphicsExtensions
	{

		public static Rectangle ToRectangle(this Gdk.Rectangle it)
			=> new(it.X, it.Y, it.Width, it.Height);
		public static Gdk.Rectangle ToNative(this Rectangle it)
			=> new Gdk.Rectangle((int)it.X, (int)it.Y, (int)it.Width, (int)it.Height);
	}

}