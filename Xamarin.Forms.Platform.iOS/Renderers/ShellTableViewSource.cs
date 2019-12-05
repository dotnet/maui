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
		DataTemplate _defaultItemTemplate;
		DataTemplate _defaultMenuItemTemplate;
		List<List<Element>> _groups;
		Dictionary<Element, View> _views;

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

		protected virtual DataTemplate DefaultItemTemplate =>
			_defaultItemTemplate ?? (_defaultItemTemplate = new DataTemplate(() => GenerateDefaultCell("Title", "FlyoutIcon")));

		protected virtual DataTemplate DefaultMenuItemTemplate =>
			_defaultMenuItemTemplate ?? (_defaultMenuItemTemplate = new DataTemplate(() => GenerateDefaultCell("Text", "Icon")));

		public void ClearCache()
		{
			_groups = null;
			_views = null;
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

				if(request.Request.Height > defaultHeight)
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

			DataTemplate template = null;
			if (context is IMenuItemController)
			{
				template = Shell.GetMenuItemTemplate(context) ?? _context.Shell.MenuItemTemplate ?? DefaultMenuItemTemplate;
			}
			else
			{
				template = Shell.GetItemTemplate(context) ?? _context.Shell.ItemTemplate ?? DefaultItemTemplate;
			}

			var cellId = ((IDataTemplateController)template.SelectDataTemplate(context, _context.Shell)).IdString;

			var cell = (UIContainerCell)tableView.DequeueReusableCell(cellId);

			if (cell == null)
			{
				var view = (View)template.CreateContent(context, _context.Shell);
				view.Parent = _context.Shell;
				cell = new UIContainerCell(cellId, view);
				cell.BindingContext = context;
			}
			else
			{
				cell.BindingContext = context;
			}

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

		View GenerateDefaultCell(string textBinding, string iconBinding)
		{
			var grid = new Grid();

			var groups = new VisualStateGroupList();

			var commonGroup = new VisualStateGroup();
			commonGroup.Name = "CommonStates";
			groups.Add(commonGroup);

			var normalState = new VisualState();
			normalState.Name = "Normal";
			commonGroup.States.Add(normalState);

			var selectedState = new VisualState();
			selectedState.Name = "Selected";
			selectedState.Setters.Add(new Setter
			{
				Property = VisualElement.BackgroundColorProperty,
				Value = new Color(0.95)
			});

			commonGroup.States.Add(selectedState);

			VisualStateManager.SetVisualStateGroups(grid, groups);

			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 50 });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

			var image = new Image();
			image.VerticalOptions = image.HorizontalOptions = LayoutOptions.Center;
			image.HeightRequest = image.WidthRequest = 22;
			image.SetBinding(Image.SourceProperty, iconBinding);
			grid.Children.Add(image);

			var label = new Label();
			label.VerticalTextAlignment = TextAlignment.Center;
			label.SetBinding(Label.TextProperty, textBinding);
			grid.Children.Add(label, 1, 0);

			label.FontSize = Device.GetNamedSize(NamedSize.Small, label);
			label.FontAttributes = FontAttributes.Bold;

			return grid;
		}

		class SeparatorView : UIView
		{
			UIView _line;

			public SeparatorView()
			{
				_line = new UIView
				{
					BackgroundColor = UIColor.Black,
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