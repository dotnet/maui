using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WPF
{
	public class CellControl : ContentControl
	{
		public static readonly DependencyProperty CellProperty = DependencyProperty.Register("Cell", typeof(object), typeof(CellControl),
			new PropertyMetadata((o, e) => ((CellControl)o).SetSource(e.OldValue, e.NewValue)));

		public static readonly DependencyProperty ShowContextActionsProperty = DependencyProperty.Register("ShowContextActions", typeof(bool), typeof(CellControl), new PropertyMetadata(true));

		readonly PropertyChangedEventHandler _propertyChangedHandler;

		public CellControl()
		{
			Unloaded += (sender, args) =>
			{
				ICellController cell = DataContext as ICellController;
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
			var renderer = Registrar.Registered.GetHandlerForObject<ICellRenderer>(cell);
			return renderer.GetTemplate(cell);
		}

		void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "HasContextActions")
				SetupContextMenu();
		}

		void SetSource(object oldCellObj, object newCellObj)
		{

			var oldCell = oldCellObj as Cell;
			var newCell = newCellObj as Cell;

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
				ContextMenuService.SetContextMenu(this, null);
				return;
			}

			ApplyTemplate();

			ContextMenu menu = new CustomContextMenu();
			menu.SetBinding(ItemsControl.ItemsSourceProperty, new System.Windows.Data.Binding("ContextActions"));

			ContextMenuService.SetContextMenu(this, menu);
		}
	}
}