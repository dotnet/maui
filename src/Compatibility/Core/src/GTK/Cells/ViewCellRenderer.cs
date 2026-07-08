using Gtk;
using Microsoft.Maui.Controls.Compatibility.Platform.GTK.Extensions;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK.Cells
{
	public class ViewCellRenderer : CellRenderer
	{
		protected override Gtk.Container GetCellWidgetInstance(Cell item)
		{
			return new ViewCell();
		}

		protected override void OnForceUpdateSizeRequest(Cell cell, Container nativeCell)
		{
			var viewCell = cell as Microsoft.Maui.Controls.Compatibility.ViewCell;
			var view = viewCell?.View;

			if (view != null)
			{
				Size request = nativeCell.GetMaxChildDesiredSize(
					view.Bounds.Width,
					cell.RenderHeight == -1
						? double.PositiveInfinity
						: cell.RenderHeight);

				cell.Height = request.Height;
			}

			base.OnForceUpdateSizeRequest(cell, nativeCell);
		}
	}
}
