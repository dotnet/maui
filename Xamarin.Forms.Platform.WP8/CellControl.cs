using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class CellControl : ContentControl
	{
		public static readonly DependencyProperty CellProperty = DependencyProperty.Register("Cell", typeof(object), typeof(CellControl),
			new PropertyMetadata((o, e) => ((CellControl)o).SetSource((Cell)e.OldValue, (Cell)e.NewValue)));

		public static readonly DependencyProperty ShowContextActionsProperty = DependencyProperty.Register("ShowContextActions", typeof(bool), typeof(CellControl), new PropertyMetadata(true));

		readonly PropertyChangedEventHandler _propertyChangedHandler;

		public CellControl()
		{
			Unloaded += (sender, args) =>
			{
				var cell = DataContext as ICellController;
				if (cell != null)
					cell.SendDisappearing();
			};

			_propertyChangedHandler = OnCellPropertyChanged;
		}

		public Cell Cell
		{
			get { return (Cell)GetValue(CellProperty); }
			set { SetValue(CellProperty, value); }
		}

		public bool ShowContextActions
		{
			get { return (bool)GetValue(ShowContextActionsProperty); }
			set { SetValue(ShowContextActionsProperty, value); }
		}

		System.Windows.DataTemplate GetTemplate(Cell cell)
		{
			var renderer = Registrar.Registered.GetHandler<ICellRenderer>(cell.GetType());
			return renderer.GetTemplate(cell);
		}

		void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "HasContextActions")
				SetupContextMenu();
		}

		void SetSource(Cell oldCell, Cell newCell)
		{
			if (oldCell != null)
			{
				oldCell.PropertyChanged -= _propertyChangedHandler;
				((ICellController)oldCell).SendDisappearing();
			}

			if (newCell != null)
			{
				((ICellController)newCell).SendAppearing();

				if (oldCell == null || oldCell.GetType() != newCell.GetType())
					ContentTemplate = GetTemplate(newCell);

				Content = newCell;

				SetupContextMenu();

				newCell.PropertyChanged += _propertyChangedHandler;
			}
			else
				Content = null;
		}

		void SetupContextMenu()
		{
			if (Content == null || !ShowContextActions)
				return;

			if (!Cell.HasContextActions)
			{
				if (VisualTreeHelper.GetChildrenCount(this) > 0)
					ContextMenuService.SetContextMenu(VisualTreeHelper.GetChild(this, 0), null);

				return;
			}

			ApplyTemplate();

			ContextMenu menu = new CustomContextMenu();
			menu.SetBinding(ItemsControl.ItemsSourceProperty, new System.Windows.Data.Binding("ContextActions"));

			ContextMenuService.SetContextMenu(VisualTreeHelper.GetChild(this, 0), menu);
		}
	}
}