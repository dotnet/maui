using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.UAP;
using UwpScrollBarVisibility = Windows.UI.Xaml.Controls.ScrollBarVisibility;
using UWPApp = Windows.UI.Xaml.Application;
using UWPDataTemplate = Windows.UI.Xaml.DataTemplate;
using System.Collections.Specialized;

namespace Xamarin.Forms.Platform.UWP
{
	public abstract class ItemsViewRenderer : ViewRenderer<ItemsView, ListViewBase>
	{
		CollectionViewSource _collectionViewSource;

		protected ListViewBase ListViewBase { get; private set; }
		UwpScrollBarVisibility? _defaultHorizontalScrollVisibility;
		UwpScrollBarVisibility? _defaultVerticalScrollVisibility;

		protected UWPDataTemplate ViewTemplate => (UWPDataTemplate)UWPApp.Current.Resources["View"];
		protected UWPDataTemplate ItemsViewTemplate => (UWPDataTemplate)UWPApp.Current.Resources["ItemsViewDefaultTemplate"];

		FrameworkElement _emptyView;
		View _formsEmptyView;

		protected ItemsControl ItemsControl { get; private set; }

		protected override void OnElementChanged(ElementChangedEventArgs<ItemsView> args)
		{
			base.OnElementChanged(args);
			TearDownOldElement(args.OldElement);
			SetUpNewElement(args.NewElement);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.Is(ItemsView.ItemsSourceProperty))
			{
				UpdateItemsSource();
			}
			else if (changedProperty.Is(ItemsView.ItemTemplateProperty))
			{
				UpdateItemTemplate();
			}
			else if (changedProperty.Is(ItemsView.HorizontalScrollBarVisibilityProperty))
			{
				UpdateHorizontalScrollBarVisibility();
			}
			else if (changedProperty.Is(ItemsView.VerticalScrollBarVisibilityProperty))
			{
				UpdateVerticalScrollBarVisibility();
			}
			else if (changedProperty.IsOneOf(ItemsView.EmptyViewProperty, ItemsView.EmptyViewTemplateProperty))
			{
				UpdateEmptyView();
			}
		}

		protected abstract ListViewBase SelectListViewBase();
		protected abstract void HandleLayoutPropertyChange(PropertyChangedEventArgs property);
		protected abstract IItemsLayout Layout { get; }

		protected virtual void UpdateItemsSource()
		{
			if (ListViewBase == null)
			{
				return;
			}

			// TODO hartez 2018-05-22 12:59 PM Handle grouping

			var itemsSource = Element.ItemsSource;

			if (itemsSource == null)
			{
				if (_collectionViewSource?.Source is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged -= ItemsChanged;
				}

				_collectionViewSource = null;
				return;
			}

			var itemTemplate = Element.ItemTemplate;

			if (_collectionViewSource != null)
			{
				if (_collectionViewSource.Source is ObservableItemTemplateCollection observableItemTemplateCollection)
				{
					observableItemTemplateCollection.CleanUp();
				}
			}

			if (itemTemplate != null)
			{
				_collectionViewSource = new CollectionViewSource
				{
					Source = TemplatedItemSourceFactory.Create(itemsSource, itemTemplate, Element),
					IsSourceGrouped = false
				};

				if (_collectionViewSource?.Source is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged += ItemsChanged;
				}
			}
			else
			{
				_collectionViewSource = new CollectionViewSource
				{
					Source = itemsSource,
					IsSourceGrouped = false
				};
			}
			
			ListViewBase.ItemsSource = _collectionViewSource.View;

			UpdateEmptyViewVisibility();
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
			HandleLayoutPropertyChange(property);
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
		}

		async void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			await ScrollTo(args);
		}

		object FindBoundItem(ScrollToRequestEventArgs args)
		{
			if (args.Mode == ScrollToMode.Position)
			{
				return _collectionViewSource.View[args.Index];
			}

			if (Element.ItemTemplate == null)
			{
				return args.Item;
			}

			for (int n = 0; n < _collectionViewSource.View.Count; n++)
			{
				if (_collectionViewSource.View[n] is ItemTemplateContext pair)
				{
					if (pair.Item == args.Item)
					{
						return _collectionViewSource.View[n];
					}
				}
			}

			return null;
		}

		async Task JumpTo(ListViewBase list, object targetItem, ScrollToPosition scrollToPosition)
		{
			var tcs = new TaskCompletionSource<object>();
			void ViewChanged(object s, ScrollViewerViewChangedEventArgs e) => tcs.TrySetResult(null);
			var scrollViewer = list.GetFirstDescendant<ScrollViewer>();

			try
			{
				scrollViewer.ViewChanged += ViewChanged;

				if (scrollToPosition == ScrollToPosition.Start)
				{
					list.ScrollIntoView(targetItem, ScrollIntoViewAlignment.Leading);
				}
				else if (scrollToPosition == ScrollToPosition.MakeVisible)
				{
					list.ScrollIntoView(targetItem, ScrollIntoViewAlignment.Default);
				}
				else
				{
					// Center and End are going to be more complicated.
				}

				await tcs.Task;
			}
			finally
			{
				scrollViewer.ViewChanged -= ViewChanged;
			}

		}

		async Task ChangeViewAsync(ScrollViewer scrollViewer, double? horizontalOffset, double? verticalOffset, bool disableAnimation)
		{
			var tcs = new TaskCompletionSource<object>();
			void ViewChanged(object s, ScrollViewerViewChangedEventArgs e) => tcs.TrySetResult(null);

			try
			{
				scrollViewer.ViewChanged += ViewChanged;
				scrollViewer.ChangeView(horizontalOffset, verticalOffset, null, disableAnimation);
				await tcs.Task;
			}
			finally
			{
				scrollViewer.ViewChanged -= ViewChanged;
			}
		}

		async Task AnimateTo(ListViewBase list, object targetItem, ScrollToPosition scrollToPosition)
		{
			var scrollViewer = list.GetFirstDescendant<ScrollViewer>();

			var targetContainer = list.ContainerFromItem(targetItem) as UIElement;

			if (targetContainer == null)
			{
				var horizontalOffset = scrollViewer.HorizontalOffset;
				var verticalOffset = scrollViewer.VerticalOffset;

				await JumpTo(list, targetItem, scrollToPosition);
				targetContainer = list.ContainerFromItem(targetItem) as UIElement;
				await ChangeViewAsync(scrollViewer, horizontalOffset, verticalOffset, true);
			}

			if (targetContainer == null)
			{
				// Did not find the target item anywhere
				return;
			}

			// TODO hartez 2018/10/04 16:37:35 Okay, this sort of works for vertical lists but fails totally on horizontal lists. 
			var transform = targetContainer.TransformToVisual(scrollViewer.Content as UIElement);
			var position = transform?.TransformPoint(new Windows.Foundation.Point(0, 0));

			if (!position.HasValue)
			{
				return;
			}

			// TODO hartez 2018/10/05 17:23:23 The animated scroll works fine vertically if we are scrolling to a greater Y offset.	
			// If we're scrolling back up to a lower Y offset, it just gives up and sends us to 0 (first item)
			// Works fine if we disable animation, but that's not very helpful

			scrollViewer.ChangeView(position.Value.X, position.Value.Y, null, false);

			//if (scrollToPosition == ScrollToPosition.End)
			//{
			//	// Modify position
			//}
			//else if (scrollToPosition == ScrollToPosition.Center)
			//{
			//	// Modify position
			//}
			//else
			//{

			//}
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

		protected virtual async Task ScrollTo(ScrollToRequestEventArgs args)
		{
			if (!(Control is ListViewBase list))
			{
				return;
			}

			var targetItem = FindBoundItem(args);

			if (args.IsAnimated)
			{
				await AnimateTo(list, targetItem, args.ScrollToPosition);
			}
			else
			{
				await JumpTo(list, targetItem, args.ScrollToPosition);
			}
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
					_emptyView = new TextBlock { Text = text };
					break;
				case View view:
					_emptyView = RealizeEmptyView(view);
					break;
				default:
					_emptyView = RealizeEmptyViewTemplate(emptyView, Element.EmptyViewTemplate);
					break;
			}

			(ListViewBase as IEmptyView)?.SetEmptyView(_emptyView);

			UpdateEmptyViewVisibility();
		}

		FrameworkElement RealizeEmptyViewTemplate(object bindingContext, DataTemplate emptyViewTemplate)
		{
			if (emptyViewTemplate == null)
			{
				return new TextBlock { Text = bindingContext.ToString() };
			}

			var template = emptyViewTemplate.SelectDataTemplate(bindingContext, null);
			var view = template.CreateContent() as View;

			view.BindingContext = bindingContext;
			return RealizeEmptyView(view);
		}

		FrameworkElement RealizeEmptyView(View view)
		{
			_formsEmptyView = view;
			return view.GetOrCreateRenderer().ContainerElement;
		}

		protected virtual void UpdateEmptyViewVisibility()
		{
			if (_emptyView != null && ListViewBase is IEmptyView emptyView)
			{
				emptyView.EmptyViewVisibility = (_collectionViewSource?.View?.Count ?? 0) == 0
					? Visibility.Visible
					: Visibility.Collapsed;

				if (emptyView.EmptyViewVisibility == Visibility.Visible)
				{
					if (ActualWidth >= 0 && ActualHeight >= 0)
					{
						_formsEmptyView?.Layout(new Rectangle(0, 0, ActualWidth, ActualHeight));
					}
				}
			}
		}
	}
}
