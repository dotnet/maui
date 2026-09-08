using System.ComponentModel;
using Microsoft.Maui.Controls.Compatibility.Internals;
using Microsoft.Maui.Controls.Compatibility.Platform.GTK.Extensions;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK.Cells
{
	public class TextCellRenderer : CellRenderer
	{
		public override CellBase GetCell(Cell item, Gtk.Container reusableView, Controls.ListView listView)
		{
			var gtkTextCell = base.GetCell(item, reusableView, listView) as TextCell;
			var textCell = (Microsoft.Maui.Controls.Compatibility.TextCell)item;

			gtkTextCell.IsGroupHeader = textCell.GetIsGroupHeader<ItemsView<Cell>, Cell>();

			return gtkTextCell;
		}

		protected override Gtk.Container GetCellWidgetInstance(Cell item)
		{
			var textCell = (Microsoft.Maui.Controls.Compatibility.TextCell)item;

			var text = textCell.Text ?? string.Empty;
			var textColor = textCell.TextColor.ToGtkColor();
			var detail = textCell.Detail ?? string.Empty;
			var detailColor = textCell.DetailColor.ToGtkColor();

			return new TextCell(
					text,
					textColor,
					detail,
					detailColor);
		}

		protected override void CellPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.CellPropertyChanged(sender, args);

			var gtkTextCell = (TextCell)sender;
			var textCell = (Microsoft.Maui.Controls.Compatibility.TextCell)gtkTextCell.Cell;

			if (args.PropertyName == Microsoft.Maui.Controls.Compatibility.TextCell.TextProperty.PropertyName)
			{
				gtkTextCell.Text = textCell.Text ?? string.Empty;
			}
			else if (args.PropertyName == Microsoft.Maui.Controls.Compatibility.TextCell.DetailProperty.PropertyName)
			{
				gtkTextCell.Detail = textCell.Detail ?? string.Empty;
			}
			else if (args.PropertyName == Microsoft.Maui.Controls.Compatibility.TextCell.TextColorProperty.PropertyName)
				gtkTextCell.TextColor = textCell.TextColor.ToGtkColor();
			else if (args.PropertyName == Microsoft.Maui.Controls.Compatibility.TextCell.DetailColorProperty.PropertyName)
				gtkTextCell.DetailColor = textCell.DetailColor.ToGtkColor();
		}
	}
}
