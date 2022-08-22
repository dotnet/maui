using System.Linq;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native.Gtk;

namespace Microsoft.Maui
{

	public static class PickerExtensions
	{

		public static Gtk.CellRendererText? GetCellRendererText(this Gtk.ComboBox? nativeView) =>
			nativeView?.Cells.FirstOrDefault() is Gtk.CellRendererText cell ? cell : null;

		public static void UpdateTextColor(this Gtk.ComboBox? nativeView, Color? color)
		{
			if (nativeView == null || color == null)
				return;

			if (nativeView.HasEntry)
				nativeView.SetColor(color, "color", "box.linked > entry.combo");

			nativeView.GetCellRendererText().SetForeground(color);
		}

		public static void UpdateHorizontalTextAlignment(this Gtk.ComboBox? nativeView, TextAlignment? alignment)
		{
			if (nativeView == null || alignment == null)
				return;

			var nativeAlign = alignment.Value.ToXyAlign();

			if (nativeView.HasEntry)
			{
				nativeView.Entry.Xalign = nativeAlign;
			}

			if (nativeView.GetCellRendererText() is { } cell)
				cell.Xalign = nativeAlign;
		}

		public static void UpdateCharacterSpacing(this Gtk.ComboBox? nativeView, double spacing)
		{
			if (nativeView == null)
				return;

			if (nativeView.HasEntry)
			{
				nativeView.Entry.Attributes = nativeView.Entry.Attributes.AttrListFor(spacing);
			}

			if (nativeView.GetCellRendererText() is { } cell)
				cell.Attributes = cell.Attributes.AttrListFor(spacing);
		}

	}

}