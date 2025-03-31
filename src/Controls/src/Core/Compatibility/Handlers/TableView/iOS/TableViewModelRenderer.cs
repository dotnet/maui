#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class TableViewModelRenderer : UITableViewSource
	{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		readonly Dictionary<nint, Cell> _headerCells = new Dictionary<nint, Cell>();
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

		protected bool HasBoundGestures;

		[Obsolete("Unused due to memory leak. Will be removed in a future version.")]
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Unused")]
		protected UITableView Table;

		[Obsolete("Unused due to memory leak. Will be removed in a future version.")]
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Unused")]
		protected TableView View;

		WeakReference<UITableView> _platformView;
#pragma warning disable CS0618 // Type or member is obsolete
		WeakReference<TableView> _tableView;
#pragma warning restore CS0618 // Type or member is obsolete

		UITableView PlatformView
		{
			get => _platformView is not null && _platformView.TryGetTarget(out var t) ? t : null;
			set => _platformView = value is not null ? new(value) : null;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		internal TableView TableView
#pragma warning restore CS0618 // Type or member is obsolete
		{
			get => _tableView is not null && _tableView.TryGetTarget(out var t) ? t : null;
			set => _tableView = value is not null ? new(value) : null;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public TableViewModelRenderer(TableView model)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			TableView = model;
			model.ModelChanged += (s, e) => PlatformView?.ReloadData();
			AutomaticallyDeselect = true;
		}

		public bool AutomaticallyDeselect { get; set; }

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (TableView is not TableView table)
				return null;
#pragma warning restore CS0618 // Type or member is obsolete
			var cell = table.Model.GetCell(indexPath.Section, indexPath.Row);
			var nativeCell = CellTableViewCell.GetPlatformCell(tableView, cell);

			return nativeCell;
		}

		public override nfloat GetHeightForHeader(UITableView tableView, nint section)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (TableView is not TableView table)
				return 0;
#pragma warning restore CS0618 // Type or member is obsolete
			if (!_headerCells.ContainsKey((int)section))
				_headerCells[section] = table.Model.GetHeaderCell((int)section);

			var result = _headerCells[section];

			return result == null ? UITableView.AutomaticDimension : (nfloat)result.Height;
		}

		public override UIView GetViewForHeader(UITableView tableView, nint section)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (TableView is not TableView table)
				return null;
#pragma warning restore CS0618 // Type or member is obsolete
			if (!_headerCells.ContainsKey((int)section))
				_headerCells[section] = table.Model.GetHeaderCell((int)section);

			var result = _headerCells[section];

			if (result != null)
			{
				var reusable = tableView.DequeueReusableCell(result.GetType().FullName);

				result.Handler?.DisconnectHandler();
				result.ReusableCell = reusable;
				result.TableView = tableView;

				var cellRenderer = result.ToHandler(table.FindMauiContext());
				return (UIView)cellRenderer.PlatformView;
			}
			return null;
		}

		public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (headerView is UITableViewHeaderFooterView header && TableView is TableView table)
			{
				var sectionHeaderTextColor = table.Model.GetSectionTextColor((int)section);
				if (sectionHeaderTextColor is not null)
				{
#pragma warning disable CA1416, CA1422 // TODO:  'UITableViewHeaderFooterView.TextLabel' is unsupported on: 'ios' 14.0 and later
					if (header.TextLabel is not null)
					{
						header.TextLabel.TextColor = sectionHeaderTextColor.ToPlatform();
					}
#pragma warning restore CA1416, CA1422
				}
			}
#pragma warning restore CS0618 // Type or member is obsolete
		}

		public void LongPress(UILongPressGestureRecognizer gesture)
		{
			if (PlatformView is not UITableView tableView)
				return;

			var point = gesture.LocationInView(tableView);
			var indexPath = tableView.IndexPathForRowAtPoint(point);
			if (indexPath == null)
				return;

			TableView?.Model.RowLongPressed(indexPath.Section, indexPath.Row);
		}

		public override nint NumberOfSections(UITableView tableView)
		{
			BindGestures(tableView);
			return TableView?.Model.GetSectionCount() ?? 0;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			TableView?.Model.RowSelected(indexPath.Section, indexPath.Row);
			if (AutomaticallyDeselect)
				tableView.DeselectRow(indexPath, true);
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return TableView?.Model.GetRowCount((int)section) ?? 0;
		}

		public override string[] SectionIndexTitles(UITableView tableView)
		{
			return TableView?.Model.GetSectionIndexTitles();
		}

		public override string TitleForHeader(UITableView tableView, nint section)
		{
			return TableView?.Model.GetSectionTitle((int)section);
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

			PlatformView = tableview;
		}

		void Tap(UITapGestureRecognizer gesture)
		{
			gesture.View.EndEditing(true);
		}
	}

	public class UnEvenTableViewModelRenderer : TableViewModelRenderer
	{
#pragma warning disable CS0618 // Type or member is obsolete
		public UnEvenTableViewModelRenderer(TableView model) : base(model)
#pragma warning restore CS0618 // Type or member is obsolete
		{
		}

		public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (TableView is not TableView table)
				return 0;
#pragma warning restore CS0618 // Type or member is obsolete

			var cell = table.Model.GetCell(indexPath.Section, indexPath.Row);
			var h = cell.Height;

#pragma warning disable CS0618 // Type or member is obsolete
			if (table.RowHeight == -1 && h == -1 && cell is ViewCell)
			{
				return UITableView.AutomaticDimension;
			}
			else if (h == -1)
				return tableView.RowHeight;
#pragma warning restore CS0618 // Type or member is obsolete
			return (nfloat)h;
		}
	}
}
