using Foundation;
using System;
using System.Collections.Generic;
using UIKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellTableViewSource : UITableViewSource
	{
		readonly IShellContext _context;
		readonly Action<Element> _onElementSelected;
		List<List<Element>> _groups;
		Dictionary<Element, View> _views;

		IShellController ShellController => (IShellController)_context.Shell;

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
					_groups = ((IShellController)_context.Shell).GenerateFlyoutGrouping();
					_views = new Dictionary<Element, View>();
				}

				return _groups;
			}
		}

		protected virtual DataTemplate DefaultItemTemplate => null;

		protected virtual DataTemplate DefaultMenuItemTemplate => null;

		public void ClearCache()
		{
			var newGroups = ((IShellController)_context.Shell).GenerateFlyoutGrouping();

			if(newGroups != _groups)
			{
				_groups = newGroups;
				_views = new Dictionary<Element, View>();
			}
		}

		public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
		{
			int section = indexPath.Section;
			int row = indexPath.Row;
			var context = Groups[section][row];
			View view;

			if (!_views.TryGetValue(context, out view))
				return UITableView.AutomaticDimension;

			nfloat defaultHeight = tableView.EstimatedRowHeight == -1 ? 44 : tableView.EstimatedRowHeight;
			nfloat height = -1;

			if (view.HeightRequest >= 0)
				height = (float)view.HeightRequest;
			else
			{
				var request = view.Measure(tableView.Bounds.Width, double.PositiveInfinity, MeasureFlags.None);

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

			var cell = (UIContainerCell)tableView.DequeueReusableCell(cellId);

			if (cell == null)
			{
				var view = (View)template.CreateContent(context, _context.Shell);
				cell = new UIContainerCell(cellId, view);
				
				// Set Parent after binding context so parent binding context doesn't propagate to view
				cell.BindingContext = context;
				view.Parent = _context.Shell;
			}
			else
			{
				cell.BindingContext = context;
			}

			cell.SetAccessibilityProperties(context);

			_views[context] = cell.View;
			return cell;
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