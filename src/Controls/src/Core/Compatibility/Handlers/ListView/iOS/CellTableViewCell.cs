#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Compatibility;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class CellTableViewCell : UITableViewCell, INativeElementView
	{
		WeakReference<Cell> _cell;

		public Action<object, PropertyChangedEventArgs> PropertyChanged;

		bool _disposed;

		public CellTableViewCell(UITableViewCellStyle style, string key) : base(style, key)
		{
		}

		public Cell Cell
		{
			get => _cell?.GetTargetOrDefault();
			set
			{
				if (_cell is not null)
				{
					if (_cell.TryGetTarget(out var cell) && cell == value)
						return;

					if (cell is not null)
					{
						BeginInvokeOnMainThread(cell.SendDisappearing);
					}
				}

				if (value is not null)
				{
					_cell = new(value);
					BeginInvokeOnMainThread(value.SendAppearing);
				}
				else
				{
					_cell = null;
				}
			}
		}

		public Element Element => Cell;

		public void HandlePropertyChanged(object sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(sender, e);

		internal static UITableViewCell GetPlatformCell(UITableView tableView, Cell cell, bool recycleCells = false, string templateId = "")
		{
			var id = cell.GetType().FullName;

			ContextActionsCell contextCell = null;
			UITableViewCell reusableCell = null;
			if (cell.HasContextActions || recycleCells)
			{
				contextCell = (ContextActionsCell)tableView.DequeueReusableCell(ContextActionsCell.Key + templateId);
				if (contextCell == null)
				{
					contextCell = new ContextActionsCell(templateId);
					reusableCell = tableView.DequeueReusableCell(id);
				}
				else
				{
					contextCell.Close();
					reusableCell = contextCell.ContentCell;

					if (reusableCell.ReuseIdentifier.ToString() != id)
						reusableCell = null;
				}
			}
			else
				reusableCell = tableView.DequeueReusableCell(id);

			cell.ReusableCell = reusableCell;
			cell.TableView = tableView;
			var handler = cell.ToHandler(cell.FindMauiContext());
			var renderer = (handler as CellRenderer) ?? (handler.PlatformView as CellRenderer);

			var platformCell = renderer.PlatformView;

			var cellWithContent = platformCell;

			// Sometimes iOS for returns a dequeued cell whose Layer is hidden. 
			// This prevents it from showing up, so lets turn it back on!
			if (cellWithContent.Layer.Hidden)
				cellWithContent.Layer.Hidden = false;

			if (contextCell != null)
			{
				contextCell.Update(tableView, cell, platformCell);
				var viewTableCell = contextCell.ContentCell as ViewCellRenderer.ViewTableCell;
				if (viewTableCell != null)
					viewTableCell.SupressSeparator = tableView.SeparatorStyle == UITableViewCellSeparatorStyle.None;
				platformCell = contextCell;
			}

			// Because the layer was hidden we need to layout the cell by hand
			if (cellWithContent != null)
				cellWithContent.LayoutSubviews();

			return platformCell;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (Cell is Cell cell)
				{
					CellRenderer.SetRealCell(cell, null);
				}
				_cell = null;
			}

			_disposed = true;

			base.Dispose(disposing);
		}
	}
}
