using System.ComponentModel;
using System.Maui.Internals;
using System.Maui.Platform.GTK.Extensions;

namespace System.Maui.Platform.GTK.Cells
{
	public class TextCellRenderer : CellRenderer
	{
		public override CellBase GetCell(Cell item, Gtk.Container reusableView, Controls.ListView listView)
		{
			var gtkTextCell = base.GetCell(item, reusableView, listView) as TextCell;
			var textCell = (System.Maui.TextCell)item;

			gtkTextCell.IsGroupHeader = textCell.GetIsGroupHeader<ItemsView<Cell>, Cell>();

			return gtkTextCell;
		}

		protected override Gtk.Container GetCellWidgetInstance(Cell item)
		{
			var textCell = (System.Maui.TextCell)item;

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
			var textCell = (System.Maui.TextCell)gtkTextCell.Cell;

			if (args.PropertyName == System.Maui.TextCell.TextProperty.PropertyName)
			{
				gtkTextCell.Text = textCell.Text ?? string.Empty;
			}
			else if (args.PropertyName == System.Maui.TextCell.DetailProperty.PropertyName)
			{
				gtkTextCell.Detail = textCell.Detail ?? string.Empty;
			}
			else if (args.PropertyName == System.Maui.TextCell.TextColorProperty.PropertyName)
				gtkTextCell.TextColor = textCell.TextColor.ToGtkColor();
			else if (args.PropertyName == System.Maui.TextCell.DetailColorProperty.PropertyName)
				gtkTextCell.DetailColor = textCell.DetailColor.ToGtkColor();
		}
	}
}
