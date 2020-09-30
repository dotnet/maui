using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xamarin.Forms.Internals;
using WSize = System.Windows.Size;

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
				if (DataContext is ICellController cell)
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
				oldCell.Appearing -= Cell_Appearing;
				((ICellController)oldCell).SendDisappearing();
			}

			if (newCell != null)
			{
				if (oldCell == null || oldCell.GetType() != newCell.GetType())
					ContentTemplate = GetTemplate(newCell);

				Content = newCell;

				SetupContextMenu();

				newCell.PropertyChanged += _propertyChangedHandler;
				newCell.Appearing += Cell_Appearing;
				((ICellController)newCell).SendAppearing();
			}
			else
				Content = null;
		}

		void Cell_Appearing(object sender, EventArgs e)
		{
			CellLayoutContent(new WSize(ActualWidth, ActualHeight));
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			CellLayoutContent(sizeInfo.NewSize);
			base.OnRenderSizeChanged(sizeInfo);
			if (sizeInfo.WidthChanged || sizeInfo.HeightChanged)
				InvalidateMeasure();
		}

		protected override WSize MeasureOverride(WSize constraint)
		{
			constraint.Width = ActualWidth;
			var size = new WSize();
			UIElement child = GetFirstVisualChild();
			if (child != null)
			{
				child.Measure(constraint);
				size.Height = child.DesiredSize.Height;
			}

			return size;
		}

		UIElement GetFirstVisualChild()
		{
			if (VisualChildrenCount <= 0)
				return null;

			return GetVisualChild(0) as UIElement;
		}

		void CellLayoutContent(WSize size)
		{
			if (double.IsInfinity(size.Width) || double.IsInfinity(size.Height) || size.Width <= 0 || size.Height <= 0)
				return;

			if (Content is ViewCell vc)
			{
				if (vc.LogicalChildren != null && vc.LogicalChildren.Any())
				{
					foreach (var child in vc.LogicalChildren)
					{
						if (child is Layout layout)
						{
							layout.Layout(new Rectangle(layout.X, layout.Y, size.Width, size.Height));
						}
					}
				}
			}
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