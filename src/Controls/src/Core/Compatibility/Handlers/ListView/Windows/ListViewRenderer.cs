#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Devices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.ListView;
using UwpScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WApp = Microsoft.UI.Xaml.Application;
using WBinding = Microsoft.UI.Xaml.Data.Binding;
using WListView = Microsoft.UI.Xaml.Controls.ListView;
using WRect = Windows.Foundation.Rect;
using WSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public partial class ListViewRenderer : ViewRenderer<ListView, FrameworkElement>
	{
		public static PropertyMapper<ListView, ListViewRenderer> Mapper =
				new PropertyMapper<ListView, ListViewRenderer>(VisualElementRendererMapper);

		public static CommandMapper<ListView, ListViewRenderer> CommandMapper =
			new CommandMapper<ListView, ListViewRenderer>(VisualElementRendererCommandMapper);

		ITemplatedItemsView<Cell> TemplatedItemsView => Element;
		bool _collectionIsWrapped;
		IList _collection = null;
		bool _itemWasClicked;
		bool _subscribedToItemClick;
		bool _subscribedToTapped;
		bool _disposed;
		CollectionViewSource _collectionViewSource;

		UwpScrollBarVisibility? _defaultHorizontalScrollVisibility;
		UwpScrollBarVisibility? _defaultVerticalScrollVisibility;

		protected WListView List { get; private set; }

		public ListViewRenderer() : base(Mapper, CommandMapper)
		{
			AutoPackage = false;
		}

		internal sealed partial class ListViewTransparent : WListView
		{
			internal ListViewRenderer ListViewRenderer { get; }
			public ListViewTransparent(ListViewRenderer listViewRenderer) : base()
			{
				this.ApplyListViewStyles();
				ListViewRenderer = listViewRenderer;
			}

			// Container is not created when the item is null. 
			// To prevent this, base container preparationan receives an empty object.
			protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
				=> base.PrepareContainerForItemOverride(element, item ?? new object());

			protected override AutomationPeer OnCreateAutomationPeer()
			{
				var automationPeer = new ListViewAutomationPeer(this);
				// skip this renderer from automationPeer tree to avoid infinity loop
				automationPeer.SetParent(new FrameworkElementAutomationPeer(Parent as FrameworkElement));
				return automationPeer;
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				e.OldElement.ItemSelected -= OnElementItemSelected;
				e.OldElement.ScrollToRequested -= OnElementScrollToRequested;
				((ITemplatedItemsView<Cell>)e.OldElement).TemplatedItems.CollectionChanged -= OnCollectionChanged;
				if (Control != null)
				{
					Control.Loaded -= ControlOnLoaded;
				}
			}

			if (e.NewElement != null)
			{
				e.NewElement.ItemSelected += OnElementItemSelected;
				e.NewElement.ScrollToRequested += OnElementScrollToRequested;
				((ITemplatedItemsView<Cell>)e.NewElement).TemplatedItems.CollectionChanged += OnCollectionChanged;

				if (List == null)
				{
					List = new ListViewTransparent(this)
					{
						IsSynchronizedWithCurrentItem = false,
						ItemTemplate = (Microsoft.UI.Xaml.DataTemplate)WApp.Current.Resources["CellTemplate"],
						HeaderTemplate = (Microsoft.UI.Xaml.DataTemplate)WApp.Current.Resources["View"],
						FooterTemplate = (Microsoft.UI.Xaml.DataTemplate)WApp.Current.Resources["View"],
						ItemContainerStyle = (Microsoft.UI.Xaml.Style)WApp.Current.Resources["MauiListViewItem"],
						GroupStyleSelector = (GroupStyleSelector)WApp.Current.Resources["ListViewGroupSelector"]
					};

					List.SelectionChanged += OnControlSelectionChanged;
					SetNativeControl(List);
				}

				ReloadData();

				UpdateGrouping();
				UpdateHeader();
				UpdateFooter();
				UpdateSelectionMode();
				UpdateWindowsSpecificSelectionMode();
				ClearSizeEstimate();
				UpdateVerticalScrollBarVisibility();
				UpdateHorizontalScrollBarVisibility();

				if (Control != null)
				{
					Control.Loaded += ControlOnLoaded;
				}
			}
		}

		void ControlOnLoaded(object sender, RoutedEventArgs e)
		{
			var scrollViewer = GetScrollViewer();
			scrollViewer?.RegisterPropertyChangedCallback(ScrollViewer.VerticalOffsetProperty, (o, dp) =>
			{
				var args = new ScrolledEventArgs(0, _scrollViewer.VerticalOffset);
				Element?.SendScrolled(args);
			});
		}

		bool IsObservableCollection(object source)
		{
			return source is INotifyCollectionChanged && source is IList;
		}

		void ReloadData()
		{
			var isStillTheSameUnderlyingItemsSource = _collection != null && object.ReferenceEquals(_collection, Element?.ItemsSource);

			if (Element?.ItemsSource == null)
			{
				_collection = null;
			}
			else
			{
				_collectionIsWrapped = !IsObservableCollection(Element.ItemsSource);

				if (_collectionIsWrapped)
				{
					_collection = new ObservableCollection<object>();
					foreach (var item in Element.ItemsSource)
						_collection.Add(item);
				}
				else if (!object.ReferenceEquals(_collection, Element.ItemsSource))
				{
					_collection = (IList)Element.ItemsSource;
				}
			}

			if (isStillTheSameUnderlyingItemsSource && _collectionViewSource != null)
				return;

			if (_collectionViewSource != null)
				_collectionViewSource.Source = null;

			_collectionViewSource = new CollectionViewSource
			{
				Source = _collection,
				IsSourceGrouped = Element.IsGroupingEnabled
			};

			List.ItemsSource = _collectionViewSource.View;
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_collectionIsWrapped && _collection != null)
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
						if (e.NewStartingIndex >= _collection.Count)
						{
							for (int i = 0; i < e.NewItems.Count; i++)
								_collection.Add((e.NewItems[i] as BindableObject).BindingContext);
						}
						else
						{
							for (int i = e.NewItems.Count - 1; i >= 0; i--)
								_collection.Insert(e.NewStartingIndex, (e.NewItems[i] as BindableObject).BindingContext);
						}

						break;
					case NotifyCollectionChangedAction.Remove:
						for (int i = e.OldItems.Count - 1; i >= 0; i--)
							_collection.RemoveAt(e.OldStartingIndex);
						break;
					case NotifyCollectionChangedAction.Move:
						{
							var collection = (ObservableCollection<object>)_collection;
							for (var i = 0; i < e.OldItems.Count; i++)
							{
								var oldi = e.OldStartingIndex;
								var newi = e.NewStartingIndex;

								if (e.NewStartingIndex < e.OldStartingIndex)
								{
									oldi += i;
									newi += i;
								}

								collection.Move(oldi, newi);
							}
						}
						break;
					case NotifyCollectionChangedAction.Replace:
						{
							var collection = (ObservableCollection<object>)_collection;
							var newi = e.NewStartingIndex;
							for (var i = 0; i < e.NewItems.Count; i++)
							{
								newi += i;
								collection[newi] = (e.NewItems[i] as BindableObject).BindingContext;
							}
						}
						break;
					case NotifyCollectionChangedAction.Reset:
					default:
						ClearSizeEstimate();
						ReloadData();
						break;
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				ClearSizeEstimate();
				ReloadData();
			}
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
			else if (e.PropertyName == ListView.SelectionModeProperty.PropertyName)
			{
				UpdateSelectionMode();
			}
			else if (e.PropertyName == Specifics.SelectionModeProperty.PropertyName)
			{
				UpdateWindowsSpecificSelectionMode();
			}
			else if (e.PropertyName == ListView.VerticalScrollBarVisibilityProperty.PropertyName)
			{
				UpdateVerticalScrollBarVisibility();
			}
			else if (e.PropertyName == ListView.HorizontalScrollBarVisibilityProperty.PropertyName)
			{
				UpdateHorizontalScrollBarVisibility();
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
				CleanUpResources();
			}

			base.Dispose(disposing);
		}

		void CleanUpResources()
		{
			if (List != null)
			{
				foreach (ViewToHandlerConverter.WrapperControl wrapperControl in FindDescendants<ViewToHandlerConverter.WrapperControl>(List))
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
				if (_collectionViewSource != null)
					_collectionViewSource.Source = null;

				List.DataContext = null;

				// Leaving this here as a warning because setting this to null causes
				// an AccessViolationException if you run Issue1975
				// List.ItemsSource = null;

				List = null;
			}

			if (_zoom != null)
			{
				_zoom.ViewChangeCompleted -= OnViewChangeCompleted;
				_zoom = null;
			}
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

			if (_collectionViewSource != null)
				_collectionViewSource.IsSourceGrouped = grouping;

			var templatedItems = TemplatedItemsView.TemplatedItems;
			if (grouping && templatedItems.ShortNames != null)
			{
				if (_zoom == null)
				{
					ScrollViewer.SetIsVerticalScrollChainingEnabled(List, false);

					var grid = new GridView { ItemsSource = templatedItems.ShortNames, Style = (Microsoft.UI.Xaml.Style)WApp.Current.Resources["JumpListGrid"] };

					ScrollViewer.SetIsHorizontalScrollChainingEnabled(grid, false);

					_zoom = new SemanticZoom { IsZoomOutButtonEnabled = false, ZoomedOutView = grid };

					// Since we reuse our ScrollTo, we have to wait until the change completes or ChangeView has odd behavior.
					_zoom.ViewChangeCompleted += OnViewChangeCompleted;

					_zoom.ZoomedInView = List;
				}
				else
				{
					_zoom.CanChangeViews = true;
				}

				SetNativeControl(_zoom);
			}
			else
			{
				if (_zoom != null)
					_zoom.CanChangeViews = false;
			}
		}

		void UpdateSelectionMode()
		{
			if (Element.SelectionMode == ListViewSelectionMode.None)
			{
				List.SelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode.None;
				List.SelectedIndex = -1;
				Element.SelectedItem = null;
			}
			else if (Element.SelectionMode == ListViewSelectionMode.Single)
			{
				List.SelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode.Single;

				// UWP seems to reset the selected item when SelectionMode is set, make sure our items stays selected by doing this call
				if (Element.SelectedItem != null)
					OnElementItemSelected(null, new SelectedItemChangedEventArgs(Element.SelectedItem, TemplatedItemsView.TemplatedItems.GetGlobalIndexOfItem(Element.SelectedItem)));
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

		void UpdateVerticalScrollBarVisibility()
		{
			if (_defaultVerticalScrollVisibility == null)
				_defaultVerticalScrollVisibility = ScrollViewer.GetVerticalScrollBarVisibility(Control);

			switch (Element.VerticalScrollBarVisibility)
			{
				case (ScrollBarVisibility.Always):
					ScrollViewer.SetVerticalScrollBarVisibility(Control, UwpScrollBarVisibility.Visible);
					break;
				case (ScrollBarVisibility.Never):
					ScrollViewer.SetVerticalScrollBarVisibility(Control, UwpScrollBarVisibility.Hidden);
					break;
				case (ScrollBarVisibility.Default):
					ScrollViewer.SetVerticalScrollBarVisibility(Control, (UwpScrollBarVisibility)_defaultVerticalScrollVisibility);
					break;
			}
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			if (_defaultHorizontalScrollVisibility == null)
				_defaultHorizontalScrollVisibility = ScrollViewer.GetHorizontalScrollBarVisibility(Control);

			switch (Element.HorizontalScrollBarVisibility)
			{
				case (ScrollBarVisibility.Always):
					ScrollViewer.SetHorizontalScrollBarVisibility(Control, UwpScrollBarVisibility.Visible);
					break;
				case (ScrollBarVisibility.Never):
					ScrollViewer.SetHorizontalScrollBarVisibility(Control, UwpScrollBarVisibility.Hidden);
					break;
				case (ScrollBarVisibility.Default):
					ScrollViewer.SetHorizontalScrollBarVisibility(Control, (UwpScrollBarVisibility)_defaultHorizontalScrollVisibility);
					break;
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
			if (DeviceInfo.Idiom == DeviceIdiom.Phone)
				await Task.Delay(1);

			IListProxy listProxy = til.ListProxy;
			ScrollTo(listProxy.ProxiedEnumerable, listProxy[0], ScrollToPosition.Start, true, true);
		}

		bool ScrollToItemWithAnimation(ScrollViewer viewer, object item)
		{
			var selectorItem = List.ContainerFromItem(item) as Microsoft.UI.Xaml.Controls.Primitives.SelectorItem;
			var transform = selectorItem?.TransformToVisual(viewer.Content as UIElement);
			var position = transform?.TransformPoint(new global::Windows.Foundation.Point(0, 0));
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
				loadedHandler = (o, e) =>
				{
					List.Loaded -= loadedHandler;

					// Here we try to avoid an exception, see explanation at bottom
					MainThread.BeginInvokeOnMainThread(() => { ScrollTo(group, item, toPosition, shouldAnimate, includeGroup); });
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

			// NOTE: For now, WinUI Dispatcher and CoreDisptacher are null. We use DispatcherQueue instead.
			Control.DispatcherQueue.TryEnqueue(UI.Dispatching.DispatcherQueuePriority.Normal, () =>
			{
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
							content.Measure(new global::Windows.Foundation.Size(viewer.ActualWidth, double.PositiveInfinity));

							double tHeight = content.DesiredSize.Height;

							if (toPosition == ScrollToPosition.Center)
								semanticLocation.Bounds = new WRect(0, viewportHeight / 2 - tHeight / 2, 0, 0);
							else
								semanticLocation.Bounds = new WRect(0, viewportHeight - tHeight, 0, 0);

							break;
						}
				}
			});

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
			Element.NotifyRowTapped(index);
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

		private protected override void DisconnectHandlerCore()
		{
			CleanUpResources();
			base.DisconnectHandlerCore();
		}

		void OnControlSelectionChanged(object sender, WSelectionChangedEventArgs e)
		{
			bool areEqual = false;

			if (Element.SelectedItem != null && Element.SelectedItem.GetType().IsValueType)
				areEqual = Element.SelectedItem.Equals(List.SelectedItem);
			else
				areEqual = Element.SelectedItem == List.SelectedItem;

			if (!areEqual)
			{
				if (_itemWasClicked)
					List.SelectedItem = Element.SelectedItem;
				else
					((IElementController)Element).SetValueFromRenderer(ListView.SelectedItemProperty, List.SelectedItem);
			}

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
	}
}
