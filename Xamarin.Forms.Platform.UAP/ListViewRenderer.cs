using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using WListView = Windows.UI.Xaml.Controls.ListView;
using WBinding = Windows.UI.Xaml.Data.Binding;
using WApp = Windows.UI.Xaml.Application;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using Specifics = Xamarin.Forms.PlatformConfiguration.WindowsSpecific.ListView;
using System.Collections.ObjectModel;

namespace Xamarin.Forms.Platform.UWP
{
	public class ListViewRenderer : ViewRenderer<ListView, FrameworkElement>
	{
		ITemplatedItemsView<Cell> TemplatedItemsView => Element;
		ObservableCollection<object> SourceItems => _context?.Source as ObservableCollection<object>;
		CollectionViewSource _context;
		bool _itemWasClicked;
		bool _subscribedToItemClick;
		bool _subscribedToTapped;
		bool _disposed;

		protected WListView List { get; private set; }

		protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				e.OldElement.ItemSelected -= OnElementItemSelected;
				e.OldElement.ScrollToRequested -= OnElementScrollToRequested;
				((ITemplatedItemsView<Cell>)e.OldElement).TemplatedItems.CollectionChanged -= OnCollectionChanged;
			}

			if (e.NewElement != null)
			{
				e.NewElement.ItemSelected += OnElementItemSelected;
				e.NewElement.ScrollToRequested += OnElementScrollToRequested;
				((ITemplatedItemsView<Cell>)e.NewElement).TemplatedItems.CollectionChanged += OnCollectionChanged;

				if (List == null)
				{
					List = new WListView
					{
						IsSynchronizedWithCurrentItem = false,
						ItemTemplate = (Windows.UI.Xaml.DataTemplate)WApp.Current.Resources["CellTemplate"],
						HeaderTemplate = (Windows.UI.Xaml.DataTemplate)WApp.Current.Resources["View"],
						FooterTemplate = (Windows.UI.Xaml.DataTemplate)WApp.Current.Resources["View"],
						ItemContainerStyle = (Windows.UI.Xaml.Style)WApp.Current.Resources["FormsListViewItem"],
						GroupStyleSelector = (GroupStyleSelector)WApp.Current.Resources["ListViewGroupSelector"]
					};

					List.SelectionChanged += OnControlSelectionChanged;

					List.SetBinding(ItemsControl.ItemsSourceProperty, "");
				}

				ReloadData();

				if (Element.SelectedItem != null)
					OnElementItemSelected(null, new SelectedItemChangedEventArgs(Element.SelectedItem));

				UpdateGrouping();
				UpdateHeader();
				UpdateFooter();
				UpdateSelectionMode();
				UpdateWindowsSpecificSelectionMode();
				ClearSizeEstimate();
			}
		}

		void ReloadData()
		{
			if (Element?.ItemsSource == null && _context != null)
				_context.Source = null;

			var allSourceItems = new ObservableCollection<object>();

			if (Element?.ItemsSource != null)
			{
				foreach (var item in Element.ItemsSource)
					allSourceItems.Add(item);
			}

			// WinRT throws an exception if you set ItemsSource directly to a CVS, so bind it.
			List.DataContext = _context = new CollectionViewSource
			{
				Source = allSourceItems,
				IsSourceGrouped = Element.IsGroupingEnabled
			};
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewStartingIndex < 0)
						goto case NotifyCollectionChangedAction.Reset;

					// if a NewStartingIndex that's too high is passed in just add the items.
					// I realize this is enforcing bad behavior but prior to this synchronization
					// code being added it wouldn't cause the app to crash whereas now it does
					// so this code accounts for that in order to ensure smooth sailing for the user
					if (e.NewStartingIndex >= SourceItems.Count)
					{
						for (int i = 0; i < e.NewItems.Count; i++)
							SourceItems.Add((e.NewItems[i] as BindableObject).BindingContext);
					}
					else
					{
						for (int i = e.NewItems.Count - 1; i >= 0; i--)
							SourceItems.Insert(e.NewStartingIndex, (e.NewItems[i] as BindableObject).BindingContext);
					}

					break;
				case NotifyCollectionChangedAction.Remove:
					for (int i = e.OldItems.Count - 1; i >= 0; i--)
						SourceItems.RemoveAt(e.OldStartingIndex);
					break;
				case NotifyCollectionChangedAction.Move:
					for (var i = 0; i < e.OldItems.Count; i++)
					{
						var oldi = e.OldStartingIndex;
						var newi = e.NewStartingIndex;

						if (e.NewStartingIndex < e.OldStartingIndex)
						{
							oldi += i;
							newi += i;
						}

						SourceItems.Move(oldi, newi);
					}
					break;
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
				default:
					ClearSizeEstimate();
					ReloadData();
					break;
			}

			Device.BeginInvokeOnMainThread(() => List?.UpdateLayout());
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ListView.IsGroupingEnabledProperty.PropertyName)
			{
				UpdateGrouping();
			}
			else if (e.PropertyName == ListView.HeaderProperty.PropertyName || e.PropertyName == "HeaderElement")
			{
				UpdateHeader();
			}
			else if (e.PropertyName == ListView.FooterProperty.PropertyName || e.PropertyName == "FooterElement")
			{
				UpdateFooter();
			}
			else if (e.PropertyName == ListView.RowHeightProperty.PropertyName)
			{
				ClearSizeEstimate();
			}
			else if (e.PropertyName == ListView.HasUnevenRowsProperty.PropertyName)
			{
				ClearSizeEstimate();
			}
			else if (e.PropertyName == ListView.ItemTemplateProperty.PropertyName)
			{
				ClearSizeEstimate();
			}
			else if (e.PropertyName == Specifics.SelectionModeProperty.PropertyName)
			{
				UpdateSelectionMode();
			}
			else if (e.PropertyName == Specifics.SelectionModeProperty.PropertyName)
			{
				UpdateWindowsSpecificSelectionMode();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				if (List != null)
				{
					foreach (ViewToRendererConverter.WrapperControl wrapperControl in FindDescendants<ViewToRendererConverter.WrapperControl>(List))
					{
						wrapperControl.CleanUp();
					}

					if (_subscribedToTapped)
					{
						_subscribedToTapped = false;
						List.Tapped -= ListOnTapped;
					}
					if (_subscribedToItemClick)
					{
						_subscribedToItemClick = false;
						List.ItemClick -= OnListItemClicked;
					}
					List.SelectionChanged -= OnControlSelectionChanged;
					List.DataContext = null;
					List = null;
				}

				if (_zoom != null)
				{
					_zoom.ViewChangeCompleted -= OnViewChangeCompleted;
					_zoom = null;
				}
			}

			base.Dispose(disposing);
		}

		static IEnumerable<T> FindDescendants<T>(DependencyObject dobj) where T : DependencyObject
		{
			int count = VisualTreeHelper.GetChildrenCount(dobj);
			for (var i = 0; i < count; i++)
			{
				DependencyObject element = VisualTreeHelper.GetChild(dobj, i);
				if (element is T)
					yield return (T)element;

				foreach (T descendant in FindDescendants<T>(element))
					yield return descendant;
			}
		}

		sealed class BrushedElement
		{
			public BrushedElement(FrameworkElement element, WBinding brushBinding = null, Brush brush = null)
			{
				Element = element;
				BrushBinding = brushBinding;
				Brush = brush;
			}

			public Brush Brush { get; }

			public WBinding BrushBinding { get; }

			public FrameworkElement Element { get; }

			public bool IsBound
			{
				get { return BrushBinding != null; }
			}
		}

		SemanticZoom _zoom;
		ScrollViewer _scrollViewer;
		ContentControl _headerControl;
		readonly List<BrushedElement> _highlightedElements = new List<BrushedElement>();

		void ClearSizeEstimate()
		{
			Element.ClearValue(CellControl.MeasuredEstimateProperty);
		}

		void UpdateFooter()
		{
			List.Footer = Element.FooterElement;
		}

		void UpdateHeader()
		{
			List.Header = Element.HeaderElement;
		}

		void UpdateGrouping()
		{
			bool grouping = Element.IsGroupingEnabled;

			if (_context != null)
				_context.IsSourceGrouped = grouping;

			var templatedItems = TemplatedItemsView.TemplatedItems;
			if (grouping && templatedItems.ShortNames != null)
			{
				if (_zoom == null)
				{
					ScrollViewer.SetIsVerticalScrollChainingEnabled(List, false);

					var grid = new GridView { ItemsSource = templatedItems.ShortNames, Style = (Windows.UI.Xaml.Style)WApp.Current.Resources["JumpListGrid"] };

					ScrollViewer.SetIsHorizontalScrollChainingEnabled(grid, false);

					_zoom = new SemanticZoom { IsZoomOutButtonEnabled = false, ZoomedOutView = grid };

					// Since we reuse our ScrollTo, we have to wait until the change completes or ChangeView has odd behavior.
					_zoom.ViewChangeCompleted += OnViewChangeCompleted;

					// Specific order to let SNC unparent the ListView for us
					SetNativeControl(_zoom);
					_zoom.ZoomedInView = List;
				}
				else
				{
					_zoom.CanChangeViews = true;
				}
			}
			else
			{
				if (_zoom != null)
					_zoom.CanChangeViews = false;
				else if (List != Control)
					SetNativeControl(List);
			}
		}

		void UpdateSelectionMode()
		{
			if (Element.SelectionMode == ListViewSelectionMode.None)
			{
				List.SelectionMode = Windows.UI.Xaml.Controls.ListViewSelectionMode.None;
				List.SelectedIndex = -1;
				Element.SelectedItem = null;
			}
			else if (Element.SelectionMode == ListViewSelectionMode.Single)
			{
				List.SelectionMode = Windows.UI.Xaml.Controls.ListViewSelectionMode.Single;
			}
		}

		void UpdateWindowsSpecificSelectionMode()
		{
			if (Element.OnThisPlatform().GetSelectionMode() == PlatformConfiguration.WindowsSpecific.ListViewSelectionMode.Accessible)
			{
				// Using Tapped will disable the ability to use the Enter key
				List.IsItemClickEnabled = true;
				if (!_subscribedToItemClick)
				{
					_subscribedToItemClick = true;
					List.ItemClick += OnListItemClicked;
				}

				if (_subscribedToTapped)
				{
					_subscribedToTapped = false;
					List.Tapped -= ListOnTapped;
				}
			}
			else
			{
				// In order to support tapping on elements within a list item, we handle 
				// ListView.Tapped (which can be handled by child elements in the list items 
				// and prevented from bubbling up) rather than ListView.ItemClick 
				if (!_subscribedToTapped)
				{
					_subscribedToTapped = true;
					List.Tapped += ListOnTapped;
				}

				List.IsItemClickEnabled = false;
				if (_subscribedToItemClick)
				{
					_subscribedToItemClick = false;
					List.ItemClick -= OnListItemClicked;
				}
			}
		}

		async void OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
		{
			if (e.IsSourceZoomedInView)
				return;

			// HACK: Technically more than one short name could be the same, this will potentially find the wrong one in that case
			var item = (string)e.SourceItem.Item;

			var templatedItems = TemplatedItemsView.TemplatedItems;
			int index = templatedItems.ShortNames.IndexOf(item);
			if (index == -1)
				return;

			var til = templatedItems.GetGroup(index);
			if (til.Count == 0)
				return; // FIXME

			// Delay until after the SemanticZoom change _actually_ finishes, fixes tons of odd issues on Phone w/ virtualization.
			if (Device.Idiom == TargetIdiom.Phone)
				await Task.Delay(1);

			IListProxy listProxy = til.ListProxy;
			ScrollTo(listProxy.ProxiedEnumerable, listProxy[0], ScrollToPosition.Start, true, true);
		}

		bool ScrollToItemWithAnimation(ScrollViewer viewer, object item)
		{
			var selectorItem = List.ContainerFromItem(item) as Windows.UI.Xaml.Controls.Primitives.SelectorItem;
			var transform = selectorItem?.TransformToVisual(viewer.Content as UIElement);
			var position = transform?.TransformPoint(new Windows.Foundation.Point(0, 0));
			if (!position.HasValue)
				return false;
			// scroll with animation
			viewer.ChangeView(position.Value.X, position.Value.Y, null);
			return true;
		}

#pragma warning disable 1998 // considered for removal
		async void ScrollTo(object group, object item, ScrollToPosition toPosition, bool shouldAnimate, bool includeGroup = false, bool previouslyFailed = false)
#pragma warning restore 1998
		{
			ScrollViewer viewer = GetScrollViewer();
			if (viewer == null)
			{
				RoutedEventHandler loadedHandler = null;
				loadedHandler = async (o, e) =>
				{
					List.Loaded -= loadedHandler;

					// Here we try to avoid an exception, see explanation at bottom
					await Dispatcher.RunIdleAsync(args => { ScrollTo(group, item, toPosition, shouldAnimate, includeGroup); });
				};
				List.Loaded += loadedHandler;
				return;
			}
			var templatedItems = TemplatedItemsView.TemplatedItems;
			Tuple<int, int> location = templatedItems.GetGroupAndIndexOfItem(group, item);
			if (location.Item1 == -1 || location.Item2 == -1)
				return;

			object[] t = templatedItems.GetGroup(location.Item1).ItemsSource.Cast<object>().ToArray();
			object c = t[location.Item2];

			// scroll to desired item with animation
			if (shouldAnimate && ScrollToItemWithAnimation(viewer, c))
				return;

			double viewportHeight = viewer.ViewportHeight;

			var semanticLocation = new SemanticZoomLocation { Item = c };

			switch (toPosition)
			{
				case ScrollToPosition.Start:
					{
						List.ScrollIntoView(c, ScrollIntoViewAlignment.Leading);
						return;
					}

				case ScrollToPosition.MakeVisible:
					{
						List.ScrollIntoView(c, ScrollIntoViewAlignment.Default);
						return;
					}

				case ScrollToPosition.End:
				case ScrollToPosition.Center:
					{
						var content = (FrameworkElement)List.ItemTemplate.LoadContent();
						content.DataContext = c;
						content.Measure(new Windows.Foundation.Size(viewer.ActualWidth, double.PositiveInfinity));

						double tHeight = content.DesiredSize.Height;

						if (toPosition == ScrollToPosition.Center)
							semanticLocation.Bounds = new Rect(0, viewportHeight / 2 - tHeight / 2, 0, 0);
						else
							semanticLocation.Bounds = new Rect(0, viewportHeight - tHeight, 0, 0);

						break;
					}
			}

			// Waiting for loaded doesn't seem to be enough anymore; the ScrollViewer does not appear until after Loaded.
			// Even if the ScrollViewer is present, an invoke at low priority fails (E_FAIL) presumably because the items are
			// still loading. An invoke at idle sometimes work, but isn't reliable enough, so we'll just have to commit
			// treason and use a blanket catch for the E_FAIL and try again.
			try
			{
				List.MakeVisible(semanticLocation);
			}
			catch (Exception)
			{
				if (previouslyFailed)
					return;

				Task.Delay(1).ContinueWith(ct => { ScrollTo(group, item, toPosition, shouldAnimate, includeGroup, true); }, TaskScheduler.FromCurrentSynchronizationContext()).WatchForError();
			}
		}

		void OnElementScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			var scrollArgs = (ITemplatedItemsListScrollToRequestedEventArgs)e;
			ScrollTo(scrollArgs.Group, scrollArgs.Item, e.Position, e.ShouldAnimate);
		}

		T GetFirstDescendant<T>(DependencyObject element) where T : FrameworkElement
		{
			int count = VisualTreeHelper.GetChildrenCount(element);
			for (var i = 0; i < count; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(element, i);

				T target = child as T ?? GetFirstDescendant<T>(child);
				if (target != null)
					return target;
			}

			return null;
		}

		ContentControl GetHeaderControl()
		{
			if (_headerControl == null)
			{
				ScrollViewer viewer = GetScrollViewer();
				if (viewer == null)
					return null;

				var presenter = GetFirstDescendant<ItemsPresenter>(viewer);
				if (presenter == null)
					return null;

				_headerControl = GetFirstDescendant<ContentControl>(presenter);
			}

			return _headerControl;
		}

		ScrollViewer GetScrollViewer()
		{
			if (_scrollViewer == null)
			{
				_scrollViewer = List.GetFirstDescendant<ScrollViewer>();
			}

			return _scrollViewer;
		}

		void ListOnTapped(object sender, TappedRoutedEventArgs args)
		{
			var orig = args.OriginalSource as DependencyObject;
			int index = -1;

			// Work our way up the tree until we find the actual list item
			// the user tapped on

			while (orig != null && orig != List)
			{
				var lv = orig as ListViewItem;
				if (lv != null)
				{
					index = TemplatedItemsView.TemplatedItems.GetGlobalIndexOfItem(lv.Content);
					break;
				}

				orig = VisualTreeHelper.GetParent(orig);
			}

			if (index > -1)
			{
				OnListItemClicked(index);
			}
		}

		void OnElementItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if (Element == null)
				return;

			if (_deferSelection)
			{
				// If we get more than one of these, that's okay; we only want the latest one
				_deferredSelectedItemChangedEvent = new Tuple<object, SelectedItemChangedEventArgs>(sender, e);
				return;
			}

			if (e.SelectedItem == null)
			{
				List.SelectedIndex = -1;
				return;
			}
			var templatedItems = TemplatedItemsView.TemplatedItems;
			var index = 0;
			if (Element.IsGroupingEnabled)
			{
				int selectedItemIndex = templatedItems.GetGlobalIndexOfItem(e.SelectedItem);
				var leftOver = 0;
				int groupIndex = templatedItems.GetGroupIndexFromGlobal(selectedItemIndex, out leftOver);

				index = selectedItemIndex - (groupIndex + 1);
			}
			else
			{
				index = templatedItems.GetGlobalIndexOfItem(e.SelectedItem);
			}

			List.SelectedIndex = index;
		}

		void OnListItemClicked(int index)
		{
			Element.NotifyRowTapped(index, cell: null);
			_itemWasClicked = true;
		}

		void OnListItemClicked(object sender, ItemClickEventArgs e)
		{
			if (e.ClickedItem != null && TemplatedItemsView?.TemplatedItems != null)
			{
				var templatedItems = TemplatedItemsView.TemplatedItems;
				var selectedItemIndex = templatedItems.GetGlobalIndexOfItem(e.ClickedItem);

				OnListItemClicked(selectedItemIndex);
			}
		}

		void OnControlSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Element.SelectedItem != List.SelectedItem && !_itemWasClicked)
				((IElementController)Element).SetValueFromRenderer(ListView.SelectedItemProperty, List.SelectedItem);

			_itemWasClicked = false;
		}

		FrameworkElement FindElement(object cell)
		{
			foreach (CellControl selector in FindDescendants<CellControl>(List))
			{
				if (ReferenceEquals(cell, selector.DataContext))
					return selector;
			}

			return null;
		}

		bool _deferSelection = false;
		Tuple<object, SelectedItemChangedEventArgs> _deferredSelectedItemChangedEvent;

		protected override AutomationPeer OnCreateAutomationPeer()
		{
			return List == null
				? new FrameworkElementAutomationPeer(this)
				: new ListViewAutomationPeer(List);
		}

	}
}