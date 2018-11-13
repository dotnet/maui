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

namespace Xamarin.Forms.Platform.UWP
{
	public class CollectionViewRenderer : ViewRenderer<CollectionView, ItemsControl>
	{
		IItemsLayout _layout;
		CollectionViewSource _collectionViewSource;

		protected ItemsControl ItemsControl { get; private set; }

		protected override void OnElementChanged(ElementChangedEventArgs<CollectionView> args)
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
		}

		protected virtual ItemsControl SelectLayout(IItemsLayout layoutSpecification)
		{
			switch (layoutSpecification)
			{
				case GridItemsLayout gridItemsLayout:
					return CreateGridView(gridItemsLayout);
				case ListItemsLayout listItemsLayout
					when listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal:
					return CreateHorizontalListView();
			}

			// Default to a plain old vertical ListView
			return new Windows.UI.Xaml.Controls.ListView();
		}

		protected virtual void UpdateItemsSource()
		{
			if (ItemsControl == null)
			{
				return;
			}

			// TODO hartez 2018-05-22 12:59 PM Handle grouping

			var itemsSource = Element.ItemsSource;

			var itemTemplate = Element.ItemTemplate;
			if (itemTemplate != null)
			{
				// The ItemContentControls need the actual data item and the template so they can inflate the template
				// and bind the result to the data item.
				// ItemTemplateEnumerator handles pairing them up for the ItemContentControls to consume
				
				_collectionViewSource = new CollectionViewSource
				{
					Source = TemplatedItemSourceFactory.Create(itemsSource, itemTemplate),
					IsSourceGrouped = false
				};
			}
			else
			{
				_collectionViewSource = new CollectionViewSource
				{
					Source = itemsSource,
					IsSourceGrouped = false
				};	
			}

			ItemsControl.ItemsSource = _collectionViewSource.View;
		}

		protected virtual void UpdateItemTemplate()
		{
			if (Element == null || ItemsControl == null)
			{
				return;
			}

			var formsTemplate = Element.ItemTemplate;
			var itemsControlItemTemplate = ItemsControl.ItemTemplate;

			if (formsTemplate == null)
			{
				ItemsControl.ItemTemplate = null;

				if (itemsControlItemTemplate != null)
				{
					// We've removed the template; the itemssource should be updated
					// TODO hartez 2018/06/25 21:25:24 I don't love that changing the template might reset the whole itemssource. We should think about a way to make that unnecessary	
					UpdateItemsSource();
				}

				return;
			}

			// TODO hartez 2018/06/23 13:47:27 Handle DataTemplateSelector case
			// Actually, DataTemplateExtensions CreateContent might handle the selector for us

			ItemsControl.ItemTemplate =
				(Windows.UI.Xaml.DataTemplate)Windows.UI.Xaml.Application.Current.Resources["ItemsViewDefaultTemplate"];

			if (itemsControlItemTemplate == null)
			{
				// We're using a data template now, so we'll need to update the itemsource
				UpdateItemsSource();
			}
		}

		static ItemsControl CreateGridView(GridItemsLayout gridItemsLayout)
		{
			var gridView = new FormsGridView();

			if (gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
			{
				gridView.UseHorizontalItemsPanel();

				// TODO hartez 2018/06/06 12:13:38 Should this logic just be built into FormsGridView?	
				ScrollViewer.SetHorizontalScrollMode(gridView, ScrollMode.Auto);
				ScrollViewer.SetHorizontalScrollBarVisibility(gridView,
					Windows.UI.Xaml.Controls.ScrollBarVisibility.Auto);
			}
			else
			{
				gridView.UseVerticalalItemsPanel();
			}

			gridView.MaximumRowsOrColumns = gridItemsLayout.Span;

			return gridView;
		}

		static ItemsControl CreateHorizontalListView()
		{
			// TODO hartez 2018/06/05 16:18:57 Is there any performance benefit to caching the ItemsPanelTemplate lookup?	
			// TODO hartez 2018/05/29 15:38:04 Make sure the ItemsViewStyles.xaml xbf gets into the nuspec	
			var horizontalListView = new Windows.UI.Xaml.Controls.ListView()
			{
				ItemsPanel =
					(ItemsPanelTemplate)Windows.UI.Xaml.Application.Current.Resources["HorizontalListItemsPanel"]
			};

			ScrollViewer.SetHorizontalScrollMode(horizontalListView, ScrollMode.Auto);
			ScrollViewer.SetHorizontalScrollBarVisibility(horizontalListView,
				Windows.UI.Xaml.Controls.ScrollBarVisibility.Auto);

			return horizontalListView;
		}

		void LayoutOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == GridItemsLayout.SpanProperty.PropertyName)
			{
				if (ItemsControl is FormsGridView formsGridView)
				{
					formsGridView.MaximumRowsOrColumns = ((GridItemsLayout)_layout).Span;
				}
			}
		}

		void SetUpNewElement(ItemsView newElement)
		{
			if (newElement == null)
			{
				return;
			}

			if (ItemsControl == null)
			{
				ItemsControl = SelectLayout(newElement.ItemsLayout);

				_layout = newElement.ItemsLayout;
				_layout.PropertyChanged += LayoutOnPropertyChanged;

				SetNativeControl(ItemsControl);
			}

			UpdateItemTemplate();
			UpdateItemsSource();

			// Listen for ScrollTo requests
			newElement.ScrollToRequested += ScrollToRequested;
		}

		void TearDownOldElement(ItemsView oldElement)
		{
			if (oldElement == null)
			{
				return;
			}

			if (_layout != null)
			{
				// Stop tracking the old layout
				_layout.PropertyChanged -= LayoutOnPropertyChanged;
				_layout = null;
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
				if (_collectionViewSource.View[n] is ItemTemplatePair pair)
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
	}
}