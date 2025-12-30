#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Foundation;
using Windows.UI.Input;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WFlyoutBase = Microsoft.UI.Xaml.Controls.Primitives.FlyoutBase;
using WMenuFlyout = Microsoft.UI.Xaml.Controls.MenuFlyout;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public partial class CellControl : ContentControl
	{
		public static readonly DependencyProperty CellProperty = DependencyProperty.Register("Cell", typeof(object), typeof(CellControl),
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			new PropertyMetadata(null, (o, e) => ((CellControl)o).SetSource((Cell)e.OldValue, (Cell)e.NewValue)));
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

		public static readonly DependencyProperty IsGroupHeaderProperty = DependencyProperty.Register("IsGroupHeader", typeof(bool), typeof(CellControl), null);

#pragma warning disable CS0618 // Type or member is obsolete
		internal static readonly BindableProperty MeasuredEstimateProperty = BindableProperty.Create("MeasuredEstimate", typeof(double), typeof(ListView), -1d);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		readonly Lazy<ListView> _listView;
#pragma warning restore CS0618 // Type or member is obsolete
		readonly PropertyChangedEventHandler _propertyChangedHandler;
		WBrush _defaultOnColor;

		IList<MenuItem> _contextActions;
		Microsoft.UI.Xaml.DataTemplate _currentTemplate;
		bool _isListViewRealized;
		object _newValue;

		public CellControl()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			_listView = new Lazy<ListView>(GetListView);
#pragma warning restore CS0618 // Type or member is obsolete

			DataContextChanged += OnDataContextChanged;

			Loaded += OnLoaded;
			Unloaded += OnUnloaded;

			_propertyChangedHandler = OnCellPropertyChanged;
		}

		void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (Cell == null)
				return;

			// 🚀 subscribe topropertychanged
			// make sure we do not subscribe twice (because this could happen in SetSource(Cell oldCell, Cell newCell))
			Cell.PropertyChanged -= _propertyChangedHandler;
			Cell.PropertyChanged += _propertyChangedHandler;
		}

		void OnUnloaded(object sender, RoutedEventArgs e)
		{
			if (Cell == null)
				return;

			Cell.SendDisappearing();
			// 🚀 unsubscribe from propertychanged
			Cell.PropertyChanged -= _propertyChangedHandler;
			// Allows the Cell to unsubscribe from Parent.PropertyChanged
#pragma warning disable CS0618 // Type or member is obsolete
			if (Cell.Parent is ListView)
				Cell.Parent = null;
#pragma warning restore CS0618 // Type or member is obsolete
		}


#pragma warning disable CS0618 // Type or member is obsolete
		public Cell Cell
#pragma warning restore CS0618 // Type or member is obsolete
		{
#pragma warning disable CS0618 // Type or member is obsolete
			get { return (Cell)GetValue(CellProperty); }
#pragma warning restore CS0618 // Type or member is obsolete
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

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			ListView lv = _listView.Value;
#pragma warning restore CS0618 // Type or member is obsolete

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
							return new global::Windows.Foundation.Size(availableSize.Width, estimate);
					}
					else
					{
						double rowHeight = lv.RowHeight;
						if (rowHeight > -1)
							return new global::Windows.Foundation.Size(availableSize.Width, rowHeight);
					}
				}

				// This needs to return a size with a non-zero height; 
				// otherwise, it kills virtualization.
				return new global::Windows.Foundation.Size(0, Cell.DefaultCellHeight);
			}

			// Children still need measure called on them
			global::Windows.Foundation.Size result = base.MeasureOverride(availableSize);

			lv?.SetValue(MeasuredEstimateProperty, result.Height);

			SetDefaultSwitchColor();

			return result;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		ListView GetListView()
#pragma warning restore CS0618 // Type or member is obsolete
		{
			DependencyObject parent = VisualTreeHelper.GetParent(this);
			while (parent != null)
			{
				var lv = parent as ListViewRenderer.ListViewTransparent;
				if (lv != null)
				{
					_isListViewRealized = true;
					return lv.ListViewRenderer.Element;
				}

				parent = VisualTreeHelper.GetParent(parent);
			}

			return null;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		Microsoft.UI.Xaml.DataTemplate GetTemplate(Cell cell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return (UI.Xaml.DataTemplate)cell.ToHandler(cell.FindMauiContext()).PlatformView;
		}

		void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "HasContextActions")
			{
				SetupContextMenu();
			}
#pragma warning disable CS0618 // Type or member is obsolete
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection(Cell);
#pragma warning disable CS0618 // Type or member is obsolete
			else if (e.PropertyName == SwitchCell.OnProperty.PropertyName ||
				e.PropertyName == SwitchCell.OnColorProperty.PropertyName)
			{
				UpdateOnColor();
			}
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		}

		void UpdateOnColor()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (!(Cell is SwitchCell switchCell))
				return;
#pragma warning restore CS0618 // Type or member is obsolete

			var color = switchCell.OnColor.IsDefault()
				? _defaultOnColor
				: new WSolidColorBrush(switchCell.OnColor.ToWindowsColor());

			var nativeSwitch = this.GetFirstDescendant<ToggleSwitch>();

			// change fill color in switch rectangle
			var rects = nativeSwitch.GetDescendantsByName<Microsoft.UI.Xaml.Shapes.Rectangle>("SwitchKnobBounds");
			foreach (var rect in rects)
				rect.Fill = color;

			// change color in animation on PointerOver
			var grid = nativeSwitch.GetFirstDescendant<Microsoft.UI.Xaml.Controls.Grid>();
			var gridVisualStateGroups = Microsoft.UI.Xaml.VisualStateManager.GetVisualStateGroups(grid);
			Microsoft.UI.Xaml.VisualStateGroup vsGroup = null;
			foreach (var visualGroup in gridVisualStateGroups)
			{
				if (visualGroup.Name == "CommonStates")
				{
					vsGroup = visualGroup;
					break;
				}
			}
			if (vsGroup == null)
				return;

			Microsoft.UI.Xaml.VisualState vState = null;
			foreach (var visualState in vsGroup.States)
			{
				if (visualState.Name == "PointerOver")
				{
					vState = visualState;
					break;
				}
			}
			if (vState == null)
				return;

			var visualStates = vState.Storyboard.Children;
			foreach (var state in visualStates)
			{
				// in XF we were setting the MinWidth of the ToggleSwitch to zero which looks to 
				// setup the visual states of ToggleSwitch to all be ObjectAnimationUsingKeyFrames.
				// This MinWidth was removed which is why this check was added
				//
				// If you find yourself here trying to figure out a SwitchCell issue
				// Try setting the MinWidth on ToggleSwitch to zero	
				if (state is ObjectAnimationUsingKeyFrames item)
				{
					if ((string)item.GetValue(Storyboard.TargetNameProperty) == "SwitchKnobBounds")
					{
						item.KeyFrames[0].Value = color;
						break;
					}
				}
			}
		}

		void SetDefaultSwitchColor()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (_defaultOnColor == null && Cell is SwitchCell)
			{
				var nativeSwitch = this.GetFirstDescendant<ToggleSwitch>();
				var rects = nativeSwitch.GetDescendantsByName<Microsoft.UI.Xaml.Shapes.Rectangle>("SwitchKnobBounds");
				foreach (var rect in rects)
					_defaultOnColor = rect.Fill;
				UpdateOnColor();
			}
#pragma warning restore CS0618 // Type or member is obsolete
		}

		void OnContextActionsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var flyout = GetAttachedFlyout();
			if (flyout != null)
			{
				flyout.Items.Clear();
				SetupMenuItems(flyout);
			}
		}

		void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;

			// We don't want to set the Cell until the ListView is realized, just in case the 
			// Cell has an ItemTemplate. Instead, we'll store the new data item, and it will be
			// set on MeasureOverrideDelegate. However, if the parent is a TableView, we'll already 
			// have a complete Cell object to work with, so we can move ahead.
#pragma warning disable CS0618 // Type or member is obsolete
			if (_isListViewRealized || args.NewValue is Cell)
				SetCell(args.NewValue);
			else if (args.NewValue != null)
				_newValue = args.NewValue;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		private void OnCellContentRightTapped(object sender, RightTappedRoutedEventArgs e)
		{
			OpenContextMenu(e.GetPosition(relativeTo: CellContent));
			e.Handled = true;
		}

		/// <summary>
		/// To check the context, not just the text.
		/// </summary>
		WMenuFlyout GetAttachedFlyout()
		{
			if (WFlyoutBase.GetAttachedFlyout(CellContent) is WMenuFlyout flyout)
			{
				var actions = Cell.ContextActions;
				if (flyout.Items.Count != actions.Count)
					return null;

				for (int i = 0; i < flyout.Items.Count; i++)
				{
					if (flyout.Items[i].DataContext != actions[i])
						return null;
				}
				return flyout;
			}
			return null;
		}

		void OpenContextMenu(Point point)
		{
			if (GetAttachedFlyout() == null)
			{
				var flyout = new WMenuFlyout();
				SetupMenuItems(flyout);

				((INotifyCollectionChanged)Cell.ContextActions).CollectionChanged += OnContextActionsChanged;

				_contextActions = Cell.ContextActions;
				WFlyoutBase.SetAttachedFlyout(CellContent, flyout);
			}

			WFlyoutBase
				.GetAttachedFlyout(CellContent)
				.ShowAt(
					CellContent,
					new FlyoutShowOptions
					{
						Position = point,
					});
		}

		void SetCell(object newContext)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var cell = newContext as Cell;
#pragma warning restore CS0618 // Type or member is obsolete

			if (cell != null)
			{
				Cell = cell;
				return;
			}

			if (ReferenceEquals(Cell?.BindingContext, newContext))
				return;

			// If there is a ListView, load the Cell content from the ItemTemplate.
			// Otherwise, the given Cell is already a templated Cell from a TableView.
#pragma warning disable CS0618 // Type or member is obsolete
			ListView lv = _listView.Value;
#pragma warning restore CS0618 // Type or member is obsolete

			if (lv != null)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				Cell oldCell = Cell;
#pragma warning restore CS0618 // Type or member is obsolete
				bool isGroupHeader = IsGroupHeader;
				DataTemplate template = isGroupHeader ? lv.GroupHeaderTemplate : lv.ItemTemplate;
				object bindingContext = newContext;

				bool sameTemplate = false;
				if (template is DataTemplateSelector dataTemplateSelector)
				{
					template = dataTemplateSelector.SelectTemplate(bindingContext, lv);

					// 🚀 If there exists an old cell, get its data template and check
					// whether the new- and old template matches. In that case, we can recycle it
					if (oldCell?.BindingContext != null)
					{
						DataTemplate oldTemplate = dataTemplateSelector.SelectTemplate(oldCell?.BindingContext, lv);
						sameTemplate = oldTemplate == template;
					}
				}

				// Reuse cell
				var canReuseCell = Cell != null && sameTemplate;

				// 🚀 If we can reuse the cell, just reuse it...
				if (canReuseCell)
				{
					cell = Cell;
				}
				else if (template != null)
				{
#pragma warning disable CS0618 // Type or member is obsolete
					cell = template.CreateContent() as Cell;
#pragma warning restore CS0618 // Type or member is obsolete
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
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
				cell.SetIsGroupHeader<ItemsView<Cell>, Cell>(isGroupHeader);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
			}

			// 🚀 Only set the cell if it DID change
			// Note: The cleanup (SendDisappearing(), etc.) is done by the Cell propertychanged callback so we do not need to do any cleanup ourselves.

			if (Cell != cell)
				Cell = cell;

			// 🚀 Even if the cell did not change, we **must** call SendDisappearing() and SendAppearing()
			// because frameworks such as Reactive UI rely on this! (this.WhenActivated())
			else if (Cell != null)
			{
				Cell.SendDisappearing();
				Cell.SendAppearing();
			}
		}

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		void SetSource(Cell oldCell, Cell newCell)
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
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

				// 🚀 make sure we do not subscribe twice (OnLoaded!)
				newCell.PropertyChanged -= _propertyChangedHandler;
				newCell.PropertyChanged += _propertyChangedHandler;
			}
		}


		void SetupContextMenu()
		{
			if (CellContent == null || Cell == null)
				return;

			if (!Cell.HasContextActions)
			{
				CellContent.RightTapped -= OnCellContentRightTapped;
				if (_contextActions != null)
				{
					((INotifyCollectionChanged)_contextActions).CollectionChanged -= OnContextActionsChanged;
					_contextActions = null;
				}

				WFlyoutBase.SetAttachedFlyout(CellContent, null);
				return;
			}

			CellContent.RightTapped += OnCellContentRightTapped;
		}

		void SetupMenuItems(WMenuFlyout flyout)
		{
			foreach (MenuItem item in Cell.ContextActions)
			{
				var flyoutItem = new UI.Xaml.Controls.MenuFlyoutItem();
				flyoutItem.SetBinding(UI.Xaml.Controls.MenuFlyoutItem.TextProperty, "Text");
				flyoutItem.SetBinding(UI.Xaml.Controls.MenuFlyoutItem.IconProperty, "IconImageSource", new IconConverter());
				flyoutItem.Command = new MenuItemCommand(item);
				flyoutItem.DataContext = item;

				flyout.Items.Add(flyoutItem);
			}
		}

#pragma warning disable CS0618 // Type or member is obsolete
		void UpdateContent(Cell newCell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			Microsoft.UI.Xaml.DataTemplate dt = GetTemplate(newCell);
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

#pragma warning disable CS0618 // Type or member is obsolete
		void UpdateFlowDirection(Cell newCell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (newCell is ViewCell)
				return;
#pragma warning restore CS0618 // Type or member is obsolete

			this.UpdateFlowDirection(newCell.Parent as VisualElement);
		}
	}
}