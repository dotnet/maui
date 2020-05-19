using System;
using System.ComponentModel;
using System.Maui.Platform.GTK.Extensions;

namespace System.Maui.Platform.GTK.Cells
{
	public class ImageCellRenderer : CellRenderer
	{
		public override CellBase GetCell(Cell item, Gtk.Container reusableView, Controls.ListView listView)
		{
			var gtkImageCell = base.GetCell(item, reusableView, listView) as ImageCell;
			var imageCell = (System.Maui.ImageCell)item;
			SetImage(imageCell, gtkImageCell);
			return gtkImageCell;
		}

		protected override Gtk.Container GetCellWidgetInstance(Cell item)
		{
			var imageCell = (System.Maui.ImageCell)item;

			var text = imageCell.Text ?? string.Empty;
			var textColor = imageCell.TextColor.ToGtkColor();
			var detail = imageCell.Detail ?? string.Empty;
			var detailColor = imageCell.DetailColor.ToGtkColor();

			return new ImageCell(
					null,
					text,
					textColor,
					detail,
					detailColor);
		}

		protected override void CellPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.CellPropertyChanged(sender, args);

			var gtkImageCell = (ImageCell)sender;
			var imageCell = (System.Maui.ImageCell)gtkImageCell.Cell;

			if (args.PropertyName == System.Maui.TextCell.TextProperty.PropertyName)
			{
				gtkImageCell.Text = imageCell.Text ?? string.Empty;
			}
			else if (args.PropertyName == System.Maui.TextCell.DetailProperty.PropertyName)
			{
				gtkImageCell.Detail = imageCell.Detail ?? string.Empty;
			}
			else if (args.PropertyName == System.Maui.ImageCell.ImageSourceProperty.PropertyName)
			{
				SetImage(imageCell, gtkImageCell);
			}
		}

		private static void SetImage(System.Maui.ImageCell cell, ImageCell target)
		{
			target.Image = null;
			_ = cell.ApplyNativeImageAsync(System.Maui.ImageCell.ImageSourceProperty, image =>
			{
				target.Image = image;
			});
		}
	}
}
