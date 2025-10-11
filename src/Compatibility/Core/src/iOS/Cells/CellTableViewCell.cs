using System;
using System.ComponentModel;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[Obsolete("Use Microsoft.Maui.Controls.Platform.Compatibility.CellTableViewCell instead")]
	public class CellTableViewCell : UITableViewCell, INativeElementView
	{
		Cell _cell;

		public Action<object, PropertyChangedEventArgs> PropertyChanged;

		bool _disposed;

		public CellTableViewCell(UITableViewCellStyle style, string key) : base(style, key)
		{
		}

		public Cell Cell
		{
			get { return _cell; }
			set
			{
				if (_cell == value)
					return;

				if (_cell != null)
				{
					_cell.PropertyChanged -= HandlePropertyChanged;
					Device.BeginInvokeOnMainThread(_cell.SendDisappearing);
				}
				_cell = value;

				if (_cell != null)
				{
					_cell.PropertyChanged += HandlePropertyChanged;
					Device.BeginInvokeOnMainThread(_cell.SendAppearing);
				}
			}
		}

		public Element Element => Cell;

		public void HandlePropertyChanged(object sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(sender, e);

		internal static UITableViewCell GetNativeCell(UITableView tableView, Cell cell, bool recycleCells = false, string templateId = "")
		{
			var id = cell.GetType().FullName;

			var renderer = (CellRenderer)Controls.Internals.Registrar.Registered.GetHandlerForObject<IRegisterable>(cell);

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

			var nativeCell = renderer.GetCell(cell, reusableCell, tableView);

			var cellWithContent = nativeCell;

			// Sometimes iOS for returns a dequeued cell whose Layer is hidden. 
			// This prevents it from showing up, so lets turn it back on!
			if (cellWithContent.Layer.Hidden)
				cellWithContent.Layer.Hidden = false;

			if (contextCell != null)
			{
				contextCell.Update(tableView, cell, nativeCell);
				var viewTableCell = contextCell.ContentCell as ViewCellRenderer.ViewTableCell;
				viewTableCell?.SupressSeparator = tableView.SeparatorStyle == UITableViewCellSeparatorStyle.None;
				nativeCell = contextCell;
			}

			// Because the layer was hidden we need to layout the cell by hand
			cellWithContent?.LayoutSubviews();

			return nativeCell;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				PropertyChanged = null;

				if (_cell != null)
				{
					_cell.PropertyChanged -= HandlePropertyChanged;
					CellRenderer.SetRealCell(_cell, null);
				}
				_cell = null;
			}

			_disposed = true;

			base.Dispose(disposing);
		}
	}
}