using System;
using System.Collections.Generic;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class TableViewModelRenderer : UITableViewSource
	{
		readonly Dictionary<nint, Cell> _headerCells = new Dictionary<nint, Cell>();

		protected bool HasBoundGestures;
		protected UITableView Table;

		protected TableView View;

		public TableViewModelRenderer(TableView model)
		{
			View = model;
			View.ModelChanged += (s, e) =>
			{
				if (Table != null)
					Table.ReloadData();
			};
			AutomaticallyDeselect = true;
		}

		public bool AutomaticallyDeselect { get; set; }

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = View.Model.GetCell(indexPath.Section, indexPath.Row);
			var nativeCell = CellTableViewCell.GetPlatformCell(tableView, cell);

			return nativeCell;
		}

		public override nfloat GetHeightForHeader(UITableView tableView, nint section)
		{
			if (!_headerCells.ContainsKey((int)section))
				_headerCells[section] = View.Model.GetHeaderCell((int)section);

			var result = _headerCells[section];

			return result == null ? UITableView.AutomaticDimension : (nfloat)result.Height;
		}

		public override UIView GetViewForHeader(UITableView tableView, nint section)
		{
			if (!_headerCells.ContainsKey((int)section))
				_headerCells[section] = View.Model.GetHeaderCell((int)section);

			var result = _headerCells[section];

			if (result != null)
			{
				var reusable = tableView.DequeueReusableCell(result.GetType().FullName);

				result.Handler?.DisconnectHandler();
				result.ReusableCell = reusable;
				result.TableView = tableView;

				var cellRenderer = result.ToHandler(View.FindMauiContext());
				return (UIView)cellRenderer.PlatformView;
			}
			return null;
		}

		public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
		{
			if (headerView is UITableViewHeaderFooterView header)
			{
				var sectionHeaderTextColor = View.Model.GetSectionTextColor((int)section);

				if (sectionHeaderTextColor != null)
				{
#pragma warning disable CA1416, CA1422 // TODO:  'UITableViewHeaderFooterView.TextLabel' is unsupported on: 'ios' 14.0 and later
					header.TextLabel.TextColor = sectionHeaderTextColor.ToPlatform();
#pragma warning restore CA1416, CA1422
				}
			}
		}

		public void LongPress(UILongPressGestureRecognizer gesture)
		{
			var point = gesture.LocationInView(Table);
			var indexPath = Table.IndexPathForRowAtPoint(point);
			if (indexPath == null)
				return;

			View.Model.RowLongPressed(indexPath.Section, indexPath.Row);
		}

		public override nint NumberOfSections(UITableView tableView)
		{
			BindGestures(tableView);
			return View.Model.GetSectionCount();
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			View.Model.RowSelected(indexPath.Section, indexPath.Row);
			if (AutomaticallyDeselect)
				tableView.DeselectRow(indexPath, true);
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return View.Model.GetRowCount((int)section);
		}

		public override string[] SectionIndexTitles(UITableView tableView)
		{
			return View.Model.GetSectionIndexTitles();
		}

		public override string TitleForHeader(UITableView tableView, nint section)
		{
			return View.Model.GetSectionTitle((int)section);
		}

		void BindGestures(UITableView tableview)
		{
			if (HasBoundGestures)
				return;

			HasBoundGestures = true;

			var gesture = new UILongPressGestureRecognizer(LongPress);
			gesture.MinimumPressDuration = 2;
			tableview.AddGestureRecognizer(gesture);

			var dismissGesture = new UITapGestureRecognizer(Tap);
			dismissGesture.CancelsTouchesInView = false;
			tableview.AddGestureRecognizer(dismissGesture);

			Table = tableview;
		}

		void Tap(UITapGestureRecognizer gesture)
		{
			gesture.View.EndEditing(true);
		}
	}

	public class UnEvenTableViewModelRenderer : TableViewModelRenderer
	{
		public UnEvenTableViewModelRenderer(TableView model) : base(model)
		{
		}

		public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = View.Model.GetCell(indexPath.Section, indexPath.Row);
			var h = cell.Height;

			if (View.RowHeight == -1 && h == -1 && cell is ViewCell)
			{
				return UITableView.AutomaticDimension;
			}
			else if (h == -1)
				return tableView.RowHeight;
			return (nfloat)h;
		}
	}
}
