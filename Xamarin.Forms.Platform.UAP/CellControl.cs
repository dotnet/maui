using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.UWP
{
	public class CellControl : ContentControl
	{
		public static readonly DependencyProperty CellProperty = DependencyProperty.Register("Cell", typeof(object), typeof(CellControl),
			new PropertyMetadata(null, (o, e) => ((CellControl)o).SetSource((Cell)e.OldValue, (Cell)e.NewValue)));

		public static readonly DependencyProperty IsGroupHeaderProperty = DependencyProperty.Register("IsGroupHeader", typeof(bool), typeof(CellControl), null);

		internal static readonly BindableProperty MeasuredEstimateProperty = BindableProperty.Create("MeasuredEstimate", typeof(double), typeof(ListView), -1d);
		readonly Lazy<ListView> _listView;
		readonly PropertyChangedEventHandler _propertyChangedHandler;

		IList<MenuItem> _contextActions;
		Windows.UI.Xaml.DataTemplate _currentTemplate;
		bool _isListViewRealized;
		object _newValue;

		public CellControl()
		{
			_listView = new Lazy<ListView>(GetListView);

			DataContextChanged += OnDataContextChanged;

			Unloaded += (sender, args) =>
			{
				Cell?.SendDisappearing();
			};

			_propertyChangedHandler = OnCellPropertyChanged;
		}

		public Cell Cell
		{
			get { return (Cell)GetValue(CellProperty); }
			set { SetValue(CellProperty, value); }
		}

		public bool IsGroupHeader
		{
			get { return (bool)GetValue(IsGroupHeaderProperty); }
			set { SetValue(IsGroupHeaderProperty, value); }
		}

		protected FrameworkElement CellContent
		{
			get { return (FrameworkElement)Content; }
		}

		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			ListView lv = _listView.Value;

			// set the Cell now that we have a reference to the ListView, since it will have been skipped
			// on DataContextChanged.
			if (_newValue != null)
			{
				SetCell(_newValue);
				_newValue = null;
			}

			if (Content == null)
			{
				if (lv != null)
				{
					if (lv.HasUnevenRows)
					{
						var estimate = (double)lv.GetValue(MeasuredEstimateProperty);
						if (estimate > -1)
							return new Windows.Foundation.Size(availableSize.Width, estimate);
					}
					else
					{
						double rowHeight = lv.RowHeight;
						if (rowHeight > -1)
							return new Windows.Foundation.Size(availableSize.Width, rowHeight);
					}
				}

				// This needs to return a size with a non-zero height; 
				// otherwise, it kills virtualization.
				return new Windows.Foundation.Size(0, Cell.DefaultCellHeight);
			}

			// Children still need measure called on them
			Windows.Foundation.Size result = base.MeasureOverride(availableSize);

			if (lv != null)
			{
				lv.SetValue(MeasuredEstimateProperty, result.Height);
			}

			return result;
		}

		ListView GetListView()
		{
			DependencyObject parent = VisualTreeHelper.GetParent(this);
			while (parent != null)
			{
				var lv = parent as ListViewRenderer;
				if (lv != null)
				{
					_isListViewRealized = true;
					return lv.Element;
				}

				parent = VisualTreeHelper.GetParent(parent);
			}

			return null;
		}

		Windows.UI.Xaml.DataTemplate GetTemplate(Cell cell)
		{
			var renderer = Registrar.Registered.GetHandlerForObject<ICellRenderer>(cell);
			return renderer.GetTemplate(cell);
		}

		void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "HasContextActions")
			{
				SetupContextMenu();
			}
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection(Cell);
		}

		void OnClick(object sender, PointerRoutedEventArgs e)
		{
			PointerPoint point = e.GetCurrentPoint(CellContent);
			if (point.Properties.PointerUpdateKind != PointerUpdateKind.RightButtonReleased)
				return;

			OpenContextMenu();
		}

		void OnContextActionsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var flyout = FlyoutBase.GetAttachedFlyout(CellContent) as MenuFlyout;
			if (flyout != null)
			{
				flyout.Items.Clear();
				SetupMenuItems(flyout);
			}
		}

		void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			// We don't want to set the Cell until the ListView is realized, just in case the 
			// Cell has an ItemTemplate. Instead, we'll store the new data item, and it will be
			// set on MeasureOverrideDelegate. However, if the parent is a TableView, we'll already 
			// have a complete Cell object to work with, so we can move ahead.
			if (_isListViewRealized || args.NewValue is Cell)
				SetCell(args.NewValue);
			else if (args.NewValue != null)
				_newValue = args.NewValue;
		}

		void OnLongTap(object sender, HoldingRoutedEventArgs e)
		{
			if (e.HoldingState == HoldingState.Started)
				OpenContextMenu();
		}

		void OpenContextMenu()
		{
			if (FlyoutBase.GetAttachedFlyout(CellContent) == null)
			{
				var flyout = new MenuFlyout();
				SetupMenuItems(flyout);

				((INotifyCollectionChanged)Cell.ContextActions).CollectionChanged += OnContextActionsChanged;

				_contextActions = Cell.ContextActions;
				FlyoutBase.SetAttachedFlyout(CellContent, flyout);
			}

			FlyoutBase.ShowAttachedFlyout(CellContent);
		}

		void SetCell(object newContext)
		{
			var cell = newContext as Cell;

			if (cell != null)
			{
				Cell = cell;
				return;
			}

			if (ReferenceEquals(Cell?.BindingContext, newContext))
				return;

			// If there is a ListView, load the Cell content from the ItemTemplate.
			// Otherwise, the given Cell is already a templated Cell from a TableView.
			ListView lv = _listView.Value;
			if (lv != null)
			{
				bool isGroupHeader = IsGroupHeader;
				DataTemplate template = isGroupHeader ? lv.GroupHeaderTemplate : lv.ItemTemplate;
				object bindingContext = newContext;

				if (template is DataTemplateSelector)
				{
					template = ((DataTemplateSelector)template).SelectTemplate(bindingContext, lv);
				}

				if (template != null)
				{
					cell = template.CreateContent() as Cell;
				}
				else
				{
					if (isGroupHeader)
						bindingContext = lv.GetDisplayTextFromGroup(bindingContext);

					cell = lv.CreateDefaultCell(bindingContext);
				}

				// A TableView cell should already have its parent,
				// but we need to set the parent for a ListView cell.
				cell.Parent = lv;

				// Set inherited BindingContext after setting the Parent so it won't be wiped out
				BindableObject.SetInheritedBindingContext(cell, bindingContext);

				// This provides the Group Header styling (e.g., larger font, etc.) when the
				// template is loaded later.
				cell.SetIsGroupHeader<ItemsView<Cell>, Cell>(isGroupHeader);
			}

			Cell = cell;
		}

		void SetSource(Cell oldCell, Cell newCell)
		{
			if (oldCell != null)
			{
				oldCell.PropertyChanged -= _propertyChangedHandler;
				oldCell.SendDisappearing();
			}

			if (newCell != null)
			{
				newCell.SendAppearing();

				UpdateContent(newCell);
				UpdateFlowDirection(newCell);
				SetupContextMenu();

				newCell.PropertyChanged += _propertyChangedHandler;
			}
		}

		void SetupContextMenu()
		{
			if (CellContent == null || Cell == null)
				return;

			if (!Cell.HasContextActions)
			{
				CellContent.Holding -= OnLongTap;
				CellContent.PointerReleased -= OnClick;
				if (_contextActions != null)
				{
					((INotifyCollectionChanged)_contextActions).CollectionChanged -= OnContextActionsChanged;
					_contextActions = null;
				}

				FlyoutBase.SetAttachedFlyout(CellContent, null);
				return;
			}

			CellContent.PointerReleased += OnClick;
			CellContent.Holding += OnLongTap;
		}

		void SetupMenuItems(MenuFlyout flyout)
		{
			foreach (MenuItem item in Cell.ContextActions)
			{
				var flyoutItem = new MenuFlyoutItem();
				flyoutItem.SetBinding(MenuFlyoutItem.TextProperty, "Text");
				flyoutItem.Command = new MenuItemCommand(item);
				flyoutItem.DataContext = item;

				flyout.Items.Add(flyoutItem);
			}
		}

		void UpdateContent(Cell newCell)
		{
			Windows.UI.Xaml.DataTemplate dt = GetTemplate(newCell);
			if (dt != _currentTemplate || Content == null)
			{
				_currentTemplate = dt;
				Content = dt.LoadContent();
			}

			((FrameworkElement)Content).DataContext = newCell;
		}

		protected override AutomationPeer OnCreateAutomationPeer()
		{
			return new FrameworkElementAutomationPeer(this);
		}

		void UpdateFlowDirection(Cell newCell)
		{
			if (newCell is ViewCell)
				return;

			this.UpdateFlowDirection(newCell.Parent as VisualElement);
		}
	}
}