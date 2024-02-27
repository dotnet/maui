using System.Linq;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui
{

	public static class PickerExtensions
	{

		public static Gtk.CellRendererText? GetCellRendererText(this Gtk.ComboBox? platformView) =>
			platformView?.Cells.FirstOrDefault() is Gtk.CellRendererText cell ? cell : null;

		public static void UpdateTextColor(this Gtk.ComboBox? platformView, Color? color)
		{
			if (platformView == null || color == null)
				return;

			if (platformView.HasEntry)
				platformView.SetColor(color, "color", "box.linked > entry.combo");

			platformView.GetCellRendererText().SetForeground(color);
		}

		public static void UpdateHorizontalTextAlignment(this Gtk.ComboBox? platformView, TextAlignment? alignment)
		{
			if (platformView == null || alignment == null)
				return;

			var nativeAlign = alignment.Value.ToXyAlign();

			if (platformView.HasEntry)
			{
				platformView.Entry.Xalign = nativeAlign;
			}

			if (platformView.GetCellRendererText() is { } cell)
				cell.Xalign = nativeAlign;
		}

		public static void UpdateCharacterSpacing(this Gtk.ComboBox? platformView, double spacing)
		{
			if (platformView == null)
				return;

			if (platformView.HasEntry && platformView.Entry.Attributes.AttrListFor(spacing) is {} entryAttributes)
			{
				platformView.Entry.Attributes = entryAttributes;
			}

			if (platformView.GetCellRendererText() is { } cell && cell.Attributes.AttrListFor(spacing) is {} cellAttrList)
				cell.Attributes = cellAttrList;
		}

	}

}