using System;
using System.Collections.Generic;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class ShellTableViewSource : UITableViewSource
	{
		readonly IShellContext _context;
		readonly Action<Element> _onElementSelected;
		List<List<Element>> _groups;
		Dictionary<Element, UIContainerCell> _cells;
		IShellController ShellController => _context.Shell;

		public ShellTableViewSource(IShellContext context, Action<Element> onElementSelected)
		{
			_context = context;
			_onElementSelected = onElementSelected;
		}

		public event EventHandler<UIScrollView> ScrolledEvent;

		public List<List<Element>> Groups
		{
			get
			{
				if (_groups == null)
				{
					_groups = ShellController.GenerateFlyoutGrouping();

					if (_cells != null)
					{
						foreach (var cell in _cells.Values)
							cell.Disconnect(_context.Shell);
					}

					_cells = new Dictionary<Element, UIContainerCell>();
				}

				return _groups;
			}
		}

		protected virtual DataTemplate DefaultItemTemplate => null;

		protected virtual DataTemplate DefaultMenuItemTemplate => null;

		internal void ReSyncCache()
		{
			var newGroups = ((IShellController)_context.Shell).GenerateFlyoutGrouping();

			if (newGroups == _groups)
				return;

			_groups = newGroups;
			if (_cells == null)
			{
				_cells = new Dictionary<Element, UIContainerCell>();
				return;
			}

			var oldList = _cells;
			_cells = new Dictionary<Element, UIContainerCell>();

			foreach (var group in newGroups)
			{
				foreach (var element in group)
				{
					UIContainerCell result;
					if (oldList.TryGetValue(element, out result))
					{
						_cells.Add(element, result);
						oldList.Remove(element);
					}
				}
			}

			foreach (var cell in oldList.Values)
				cell.Disconnect(_context.Shell);
		}


		public void ClearCache()
		{
			var newGroups = ((IShellController)_context.Shell).GenerateFlyoutGrouping();

			if (newGroups != _groups)
			{
				_groups = newGroups;
				if (_cells != null)
				{
					foreach (var cell in _cells.Values)
						cell.Disconnect(_context.Shell);
				}
				_cells = new Dictionary<Element, UIContainerCell>();
			}
		}

		public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
		{
			int section = indexPath.Section;
			int row = indexPath.Row;
			var context = Groups[section][row];

			if (!_cells.TryGetValue(context, out var view))
				return UITableView.AutomaticDimension;

			if (!view.View.IsVisible)
				return 0;

			nfloat defaultHeight = tableView.EstimatedRowHeight == -1 ? 0 : tableView.EstimatedRowHeight;
			nfloat height = -1;

			if (view.View.HeightRequest >= 0)
				height = (float)view.View.HeightRequest;
			else
			{
				var request = view.View.Measure(tableView.Bounds.Width, double.PositiveInfinity, MeasureFlags.None);

				if (request.Request.Height > defaultHeight)
					height = (float)request.Request.Height;
				else
					height = defaultHeight;
			}

			if (height == -1)
				height = defaultHeight;

			return height;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			int section = indexPath.Section;
			int row = indexPath.Row;
			var context = Groups[section][row];

			DataTemplate template = ShellController.GetFlyoutItemDataTemplate(context);
			if (context is IMenuItemController)
			{
				if (DefaultMenuItemTemplate != null && _context.Shell.MenuItemTemplate == template)
					template = DefaultMenuItemTemplate;
			}
			else
			{
				if (DefaultItemTemplate != null && _context.Shell.ItemTemplate == template)
					template = DefaultItemTemplate;
			}

			var cellId = ((IDataTemplateController)template.SelectDataTemplate(context, _context.Shell)).IdString;

			UIContainerCell cell;
			if (!_cells.TryGetValue(context, out cell))
			{
				var view = (View)template.CreateContent(context, _context.Shell);
				cell = new UIContainerCell(cellId, view, _context.Shell, context);
			}
			else
			{
				var view = _cells[context].View;
				cell.Disconnect(keepRenderer: true);
				cell = new UIContainerCell(cellId, view, _context.Shell, context);
			}

			cell.SetAccessibilityProperties(context);

			_cells[context] = cell;
			cell.TableView = tableView;
			cell.IndexPath = indexPath;
			cell.ViewMeasureInvalidated += OnViewMeasureInvalidated;

			return cell;
		}

		void OnViewMeasureInvalidated(UIContainerCell cell)
		{
			cell.ReloadRow();
		}

		public override nfloat GetHeightForFooter(UITableView tableView, nint section)
		{
			if (section < Groups.Count - 1)
				return 1;

			return 0;
		}

		public override UIView GetViewForFooter(UITableView tableView, nint section)
		{
			return new SeparatorView();
		}

		public override nint NumberOfSections(UITableView tableView)
		{
			return Groups.Count;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			int section = indexPath.Section;
			int row = indexPath.Row;

			var element = Groups[section][row];
			_onElementSelected(element);
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return Groups[(int)section].Count;
		}

		public override void Scrolled(UIScrollView scrollView)
		{
			ScrolledEvent?.Invoke(this, scrollView);
		}

		public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
		{
			cell.BackgroundColor = UIColor.Clear;
		}

		class SeparatorView : UIView
		{
			UIView _line;

			public SeparatorView()
			{
				_line = new UIView
				{
					BackgroundColor = ColorExtensions.OpaqueSeparatorColor,
					TranslatesAutoresizingMaskIntoConstraints = true,
					Alpha = 0.2f
				};

				Add(_line);
			}

			public override void LayoutSubviews()
			{
				_line.Frame = new CoreGraphics.CGRect(15, 0, Frame.Width - 30, 1);
				base.LayoutSubviews();
			}
		}
	}
}
