using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native.Gtk;

namespace Microsoft.Maui
{

	public static class CellExtensions
	{

		public static void SetForeground(this Gtk.CellRendererText? cell, Color? color)
		{
			if (cell == null || color == null)
				return;

			cell.ForegroundRgba = color.ToGdkRgba();
		}

		public static void SetBackground(this Gtk.CellRendererText? cell, Color? color)
		{
			if (cell == null || color == null)
				return;

			cell.BackgroundRgba = color.ToGdkRgba();
		}

	}

}