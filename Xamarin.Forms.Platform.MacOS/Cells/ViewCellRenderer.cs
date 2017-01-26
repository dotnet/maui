using System.ComponentModel;
using AppKit;

// ReSharper disable UnusedParameter.Local

namespace Xamarin.Forms.Platform.MacOS
{
	public class ViewCellRenderer : CellRenderer
	{
		public override NSView GetCell(Cell item, NSView reusableView, NSTableView tv)
		{
			var viewCell = (ViewCell)item;

			var cell = reusableView as ViewCellNSView;
			if (cell == null)
				cell = new ViewCellNSView();
			else
				cell.ViewCell.PropertyChanged -= ViewCellPropertyChanged;

			viewCell.PropertyChanged += ViewCellPropertyChanged;
			cell.ViewCell = viewCell;

			SetRealCell(item, cell);

			WireUpForceUpdateSizeRequested(item, cell, tv);

			UpdateBackground(cell, item);
			UpdateIsEnabled(cell, viewCell);
			return cell;
		}

		static void UpdateIsEnabled(ViewCellNSView cell, ViewCell viewCell)
		{
			//TODO: Implement IsEnabled on ViewCell
		}

		static void ViewCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var viewCell = (ViewCell)sender;
			var realCell = (ViewCellNSView)GetRealCell(viewCell);

			if (e.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled(realCell, viewCell);
		}
	}
}