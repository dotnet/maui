using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using UwpApp = Microsoft.UI.Xaml.Application;
using UwpDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using UwpScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WRect = Windows.Foundation.Rect;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public abstract class ItemsViewRenderer<TItemsView> : ViewRenderer<TItemsView, ListViewBase>
		where TItemsView : ItemsView
	{
		protected CollectionViewSource CollectionViewSource;

		UwpScrollBarVisibility? _defaultHorizontalScrollVisibility;
		UwpScrollBarVisibility? _defaultVerticalScrollVisibility;
		FrameworkElement _emptyView;
		View _formsEmptyView;
		bool _emptyViewDisplayed;
		ScrollViewer _scrollViewer;
		double _previousHorizontalOffset;
		double _previousVerticalOffset;

		protected ListViewBase ListViewBase { get; private set; }
		protected UwpDataTemplate ViewTemplate => (UwpDataTemplate)UwpApp.Current.Resources["View"];
		protected UwpDataTemplate ItemsViewTemplate => (UwpDataTemplate)UwpApp.Current.Resources["ItemsViewDefaultTemplate"];

		protected ItemsViewRenderer()
		{
			AutoPackage = false;
		}

		protected TItemsView ItemsView => Element;
		protected ItemsControl ItemsControl { get; private set; }

		protected override void OnElementChanged(ElementChangedEventArgs<TItemsView> args)
		{
			base.OnElementChanged(args);
			TearDownOldElement(args.OldElement);
			SetUpNewElement(args.NewElement);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.Is(Microsoft.Maui.Controls.ItemsView.ItemsSourceProperty))
			{
				UpdateItemsSource();
			}
			else if (changedProperty.Is(Microsoft.Maui.Controls.ItemsView.ItemTemplateProperty))
			{
				UpdateItemTemplate();
			}
			else if (changedProperty.Is(Microsoft.Maui.Controls.ItemsView.HorizontalScrollBarVisibilityProperty))
			{
				UpdateHorizontalScrollBarVisibility();
			}
			else if (changedProperty.Is(Microsoft.Maui.Controls.ItemsView.VerticalScrollBarVisibilityProperty))
			{
				UpdateVerticalScrollBarVisibility();
			}
			else if (changedProperty.IsOneOf(Microsoft.Maui.Controls.ItemsView.EmptyViewProperty,
				Microsoft.Maui.Controls.ItemsView.EmptyViewTemplateProperty))
			{
				UpdateEmptyView();
			}
		}

		protected abstract ListViewBase SelectListViewBase();
		protected abstract void HandleLayoutPropertyChanged(PropertyChangedEventArgs property);
		protected abstract IItemsLayout Layout { get; }

		protected virtual void UpdateItemsSource()
		{
			if (ListViewBase == null)
			{
				return;
			}

			CleanUpCollectionViewSource();

			if (Element.ItemsSource == null)
			{
				return;
			}

			CollectionViewSource = CreateCollectionViewSource();

			if (CollectionViewSource?.Source is INotifyCollectionChanged incc)
			{
				incc.CollectionChanged += ItemsChanged;
			}

			ListViewBase.ItemsSource = GetCollectionView(CollectionViewSource);

			UpdateEmptyViewVisibility();
		}

		protected virtual ICollectionView GetCollectionView(CollectionViewSource collectionViewSource)
		{
			return collectionViewSource.View;
		}

		protected virtual void CleanUpCollectionViewSource()
		{
			if (CollectionViewSource != null)
			{
				if (CollectionViewSource.Source is ObservableItemTemplateCollection observableItemTemplateCollection)
				{
					observableItemTemplateCollection.Dispose();
				}

				if (CollectionViewSource.Source is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged -= ItemsChanged;
				}

				CollectionViewSource.Source = null;
				CollectionViewSource = null;
			}

			if (Element?.ItemsSource == null)
			{
				ListViewBase.ItemsSource = null;
				return;
			}
		}

		protected virtual CollectionViewSource CreateCollectionViewSource()
		{
			var itemsSource = Element.ItemsSource;
			var itemTemplate = Element.ItemTemplate;

			if (itemTemplate != null)
			{
				return new CollectionViewSource
				{
					Source = TemplatedItemSourceFactory.Create(itemsSource, itemTemplate, Element),
					IsSourceGrouped = false
				};
			}

			return new CollectionViewSource
			{
				Source = itemsSource,
				IsSourceGrouped = false
			};
		}

		void ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateEmptyViewVisibility();
		}

		protected virtual void UpdateItemTemplate()
		{
			if (Element == null || ListViewBase == null)
			{
				return;
			}

			ListViewBase.ItemTemplate = Element.ItemTemplate == null ? null : ItemsViewTemplate;

			UpdateItemsSource();
		}

		void LayoutPropertyChanged(object sender, PropertyChangedEventArgs property)
		{
			HandleLayoutPropertyChanged(property);
		}

		protected virtual void SetUpNewElement(ItemsView newElement)
		{
			if (newElement == null)
			{
				return;
			}

			if (ListViewBase == null)
			{
				ListViewBase = SelectListViewBase();
				ListViewBase.IsSynchronizedWithCurrentItem = false;

				FindScrollViewer(ListViewBase);

				Layout.PropertyChanged += LayoutPropertyChanged;

				SetNativeControl(ListViewBase);
			}

			UpdateItemTemplate();
			UpdateItemsSource();
			UpdateVerticalScrollBarVisibility();
			UpdateHorizontalScrollBarVisibility();
			UpdateEmptyView();

			// Listen for ScrollTo requests
			newElement.ScrollToRequested += ScrollToRequested;
		}

		protected virtual void TearDownOldElement(ItemsView oldElement)
		{
			if (oldElement == null)
			{
				return;
			}

			if (Layout != null)
			{
				// Stop tracking the old layout
				Layout.PropertyChanged -= LayoutPropertyChanged;
			}

			// Stop listening for ScrollTo requests
			oldElement.ScrollToRequested -= ScrollToRequested;

			if (CollectionViewSource != null)
			{
				CleanUpCollectionViewSource();
			}

			if (ListViewBase != null)
			{
				ListViewBase.ItemsSource = null;
			}

			if (_scrollViewer != null)
			{
				_scrollViewer.ViewChanged -= OnScrollViewChanged;
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
					ScrollViewer.SetVerticalScrollBarVisibility(Control, _defaultVerticalScrollVisibility.Value);
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
					ScrollViewer.SetHorizontalScrollBarVisibility(Control, _defaultHorizontalScrollVisibility.Value);
					break;
			}
		}

		void FindScrollViewer(ListViewBase listView)
		{
			var scrollViewer = listView.GetFirstDescendant<ScrollViewer>();

			if (scrollViewer != null)
			{
				OnScrollViewerFound(scrollViewer);
				return;
			}

			void ListViewLoaded(object sender, RoutedEventArgs e)
			{
				var lv = (ListViewBase)sender;
				lv.Loaded -= ListViewLoaded;
				FindScrollViewer(listView);
			}

			listView.Loaded += ListViewLoaded;
		}

		protected virtual void OnScrollViewerFound(ScrollViewer scrollViewer)
		{
			_scrollViewer = scrollViewer;
			_scrollViewer.ViewChanged += OnScrollViewChanged;
		}

		protected virtual async Task ScrollTo(ScrollToRequestEventArgs args)
		{
			if (!(Control is ListViewBase list))
			{
				return;
			}

			var item = FindBoundItem(args);

			if (item == null)
			{
				// Item wasn't found in the list, so there's nothing to scroll to
				return;
			}

			if (args.IsAnimated)
			{
				await ScrollHelpers.AnimateToItemAsync(list, item, args.ScrollToPosition);
			}
			else
			{
				await ScrollHelpers.JumpToItemAsync(list, item, args.ScrollToPosition);
			}
		}

		async void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			await ScrollTo(args);
		}

		object FindBoundItem(ScrollToRequestEventArgs args)
		{
			if (args.Mode == ScrollToMode.Position)
			{
				if (args.Index >= ItemCount)
				{
					return null;
				}

				return GetItem(args.Index);
			}

			if (Element.ItemTemplate == null)
			{
				return args.Item;
			}

			for (int n = 0; n < ItemCount; n++)
			{
				if (CollectionViewSource.View[n] is ItemTemplateContext pair)
				{
					if (pair.Item == args.Item)
					{
						return CollectionViewSource.View[n];
					}
				}
			}

			return null;
		}

		protected virtual void UpdateEmptyView()
		{
			if (Element == null || ListViewBase == null)
			{
				return;
			}

			var emptyView = Element.EmptyView;

			if (emptyView == null)
			{
				return;
			}

			switch (emptyView)
			{
				case string text:
					_emptyView = new TextBlock
					{
						HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
						VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
						Text = text
					};
					break;
				case View view:
					_emptyView = RealizeEmptyView(view);
					break;
				default:
					_emptyView = RealizeEmptyViewTemplate(emptyView, Element.EmptyViewTemplate);
					break;
			}

			(ListViewBase as IEmptyView)?.SetEmptyView(_emptyView, _formsEmptyView);

			UpdateEmptyViewVisibility();
		}

		protected virtual void UpdateItemsLayout()
		{
			if (_scrollViewer != null)
				_scrollViewer.ViewChanged -= OnScrollViewChanged;

			if (ListViewBase != null)
			{
				ListViewBase.ItemsSource = null;
				ListViewBase = null;
			}

			ListViewBase = SelectListViewBase();
			ListViewBase.IsSynchronizedWithCurrentItem = false;

			FindScrollViewer(ListViewBase);

			SetNativeControl(ListViewBase);

			_defaultHorizontalScrollVisibility = null;
			_defaultVerticalScrollVisibility = null;

			UpdateItemTemplate();
			UpdateItemsSource();
			UpdateVerticalScrollBarVisibility();
			UpdateHorizontalScrollBarVisibility();
			UpdateEmptyView();
		}

		FrameworkElement RealizeEmptyViewTemplate(object bindingContext, DataTemplate emptyViewTemplate)
		{
			if (emptyViewTemplate == null)
			{
				return new TextBlock
				{
					HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
					VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
					Text = bindingContext.ToString()
				};
			}

			var template = emptyViewTemplate.SelectDataTemplate(bindingContext, null);
			var view = template.CreateContent() as View;
			view.BindingContext = bindingContext;

			return RealizeEmptyView(view);
		}

		FrameworkElement RealizeEmptyView(View view)
		{
			_formsEmptyView = view ?? throw new ArgumentNullException(nameof(view));
			return view.GetOrCreateRenderer().ContainerElement;
		}

		protected virtual void UpdateEmptyViewVisibility()
		{
			bool isEmpty = (CollectionViewSource?.View?.Count ?? 0) == 0;

			if (isEmpty)
			{
				if (_formsEmptyView != null)
				{
					if (_emptyViewDisplayed)
						ItemsView.RemoveLogicalChild(_formsEmptyView);

					if (ItemsView.EmptyViewTemplate == null)
						ItemsView.AddLogicalChild(_formsEmptyView);
				}

				if (_emptyView != null && ListViewBase is IEmptyView emptyView)
				{
					emptyView.EmptyViewVisibility = WVisibility.Visible;

					if (ActualWidth >= 0 && ActualHeight >= 0)
						_formsEmptyView?.Layout(new Rect(0, 0, ActualWidth, ActualHeight));
				}

				_emptyViewDisplayed = true;
			}
			else
			{
				if (_emptyViewDisplayed)
				{
					if (_emptyView != null && ListViewBase is IEmptyView emptyView)
						emptyView.EmptyViewVisibility = WVisibility.Collapsed;

					ItemsView.RemoveLogicalChild(_formsEmptyView);
				}

				_emptyViewDisplayed = false;
			}
		}

		internal void HandleScroll(ScrollViewer scrollViewer)
		{
			var itemsViewScrolledEventArgs = new ItemsViewScrolledEventArgs
			{
				HorizontalOffset = scrollViewer.HorizontalOffset,
				HorizontalDelta = scrollViewer.HorizontalOffset - _previousHorizontalOffset,
				VerticalOffset = scrollViewer.VerticalOffset,
				VerticalDelta = scrollViewer.VerticalOffset - _previousVerticalOffset,
			};

			_previousHorizontalOffset = scrollViewer.HorizontalOffset;
			_previousVerticalOffset = scrollViewer.VerticalOffset;

			var layoutOrientaton = ItemsLayoutOrientation.Vertical;
			bool advancing = true;
			switch (Layout)
			{
				case LinearItemsLayout linearItemsLayout:
					layoutOrientaton = linearItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal ? ItemsLayoutOrientation.Horizontal : ItemsLayoutOrientation.Vertical;
					advancing = itemsViewScrolledEventArgs.HorizontalDelta > 0;
					break;
				case GridItemsLayout gridItemsLayout:
					layoutOrientaton = gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal ? ItemsLayoutOrientation.Horizontal : ItemsLayoutOrientation.Vertical;
					advancing = itemsViewScrolledEventArgs.VerticalDelta > 0;
					break;
				default:
					break;
			}

			itemsViewScrolledEventArgs = ComputeVisibleIndexes(itemsViewScrolledEventArgs, layoutOrientaton, advancing);

			Element.SendScrolled(itemsViewScrolledEventArgs);
		}

		protected virtual ItemsViewScrolledEventArgs ComputeVisibleIndexes(ItemsViewScrolledEventArgs args, ItemsLayoutOrientation orientation, bool advancing)
		{
			var (firstVisibleItemIndex, lastVisibleItemIndex, centerItemIndex) = GetVisibleIndexes(orientation, advancing);

			args.FirstVisibleItemIndex = firstVisibleItemIndex;
			args.CenterItemIndex = centerItemIndex;
			args.LastVisibleItemIndex = lastVisibleItemIndex;

			return args;
		}

		(int firstVisibleItemIndex, int lastVisibleItemIndex, int centerItemIndex) GetVisibleIndexes(ItemsLayoutOrientation itemsLayoutOrientation, bool advancing)
		{
			int firstVisibleItemIndex = -1;
			int lastVisibleItemIndex = -1;

			if (ListViewBase.ItemsPanelRoot is ItemsStackPanel itemsPanel)
			{
				firstVisibleItemIndex = itemsPanel.FirstVisibleIndex;
				lastVisibleItemIndex = itemsPanel.LastVisibleIndex;
			}
			else
			{
				var presenters = ListViewBase.GetChildren<ListViewItemPresenter>();

				if (presenters != null && _scrollViewer != null)
				{
					int count = 0;
					foreach (ListViewItemPresenter presenter in presenters)
					{
						if (IsElementVisibleInContainer(presenter, _scrollViewer, itemsLayoutOrientation))
						{
							if (firstVisibleItemIndex == -1)
								firstVisibleItemIndex = count;

							lastVisibleItemIndex = count;
						}

						count++;
					}
				}
			}

			double center = (lastVisibleItemIndex + firstVisibleItemIndex) / 2.0;
			int centerItemIndex = advancing ? (int)Math.Ceiling(center) : (int)Math.Floor(center);

			return (firstVisibleItemIndex, lastVisibleItemIndex, centerItemIndex);
		}

		bool IsElementVisibleInContainer(FrameworkElement element, FrameworkElement container, ItemsLayoutOrientation itemsLayoutOrientation)
		{
			if (element == null || container == null)
				return false;

			if (element.Visibility != WVisibility.Visible)
				return false;

			var elementBounds = element.TransformToVisual(container).TransformBounds(new WRect(0, 0, element.ActualWidth, element.ActualHeight));
			var containerBounds = new WRect(0, 0, container.ActualWidth, container.ActualHeight);

			switch (itemsLayoutOrientation)
			{
				case ItemsLayoutOrientation.Vertical:
					return elementBounds.Top < containerBounds.Bottom && elementBounds.Bottom > containerBounds.Top;

				default:
					return elementBounds.Left < containerBounds.Right && elementBounds.Right > containerBounds.Left;
			}
			;
		}

		void OnScrollViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			HandleScroll(_scrollViewer);
		}

		protected virtual int ItemCount => CollectionViewSource.View.Count;

		protected virtual object GetItem(int index)
		{
			return CollectionViewSource.View[index];
		}
	}
}
