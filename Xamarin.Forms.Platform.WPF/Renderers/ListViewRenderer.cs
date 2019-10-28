using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Xamarin.Forms.Platform.WPF.Helpers;
using WList = System.Windows.Controls.ListView;
using WpfScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility;

namespace Xamarin.Forms.Platform.WPF
{
	public class ListViewRenderer : ViewRenderer<ListView, WList>
	{
		class ScrollViewerBehavior
		{
			public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double),
				typeof(ScrollViewerBehavior), new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

			static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
			{
				var scrollViewer = target as ScrollViewer;
				scrollViewer?.ScrollToVerticalOffset((double)e.NewValue);
			}
		}
		ITemplatedItemsView<Cell> TemplatedItemsView => Element;
		WpfScrollBarVisibility? _defaultHorizontalScrollVisibility;
		WpfScrollBarVisibility? _defaultVerticalScrollVisibility;
		ScrollViewer _scrollViewer;

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			SizeRequest result = base.GetDesiredSize(widthConstraint, heightConstraint);
			result.Minimum = new Size(40, 40);
			return result;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
		{
			if (e.OldElement != null) // Clear old element event
			{
				e.OldElement.ItemSelected -= OnElementItemSelected;
				e.OldElement.ScrollToRequested -= OnElementScrollToRequested;

				var templatedItems = ((ITemplatedItemsView<Cell>)e.OldElement).TemplatedItems;
				templatedItems.CollectionChanged -= OnCollectionChanged;
				templatedItems.GroupedCollectionChanged -= OnGroupedCollectionChanged;
				if (Control != null)
				{
					Control.MouseUp -= OnNativeMouseUp;
					Control.KeyUp -= OnNativeKeyUp;
					Control.TouchUp -= OnNativeTouchUp;
					Control.StylusUp -= OnNativeStylusUp;
					Control.Loaded -= ControlOnLoaded;
				}
				if (_scrollViewer != null)
				{
					_scrollViewer.ScrollChanged -= SendScrolled;
				}
			}

			if (e.NewElement != null)
			{
				e.NewElement.ItemSelected += OnElementItemSelected;
				e.NewElement.ScrollToRequested += OnElementScrollToRequested;

				if (Control == null) // Construct and SetNativeControl and suscribe control event
				{
					var listView = new WList
					{
						DataContext = Element,
						ItemTemplate = (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["CellTemplate"],
						Style = (System.Windows.Style)System.Windows.Application.Current.Resources["ListViewTemplate"]
					};

					VirtualizingPanel.SetVirtualizationMode(listView, VirtualizationMode.Recycling);
					VirtualizingPanel.SetScrollUnit(listView, ScrollUnit.Pixel);
					SetNativeControl(listView);

					Control.MouseUp += OnNativeMouseUp;
					Control.KeyUp += OnNativeKeyUp;
					Control.TouchUp += OnNativeTouchUp;
					Control.StylusUp += OnNativeStylusUp;
					Control.Loaded += ControlOnLoaded;
				}

				// Suscribe element events
				var templatedItems = TemplatedItemsView.TemplatedItems;
				templatedItems.CollectionChanged += OnCollectionChanged;
				templatedItems.GroupedCollectionChanged += OnGroupedCollectionChanged;

				// Update control properties
				UpdateItemSource();
				UpdateHorizontalScrollBarVisibility();
				UpdateVerticalScrollBarVisibility();

				if (Element.SelectedItem != null)
					OnElementItemSelected(null, new SelectedItemChangedEventArgs(Element.SelectedItem, -1));
			}

			base.OnElementChanged(e);
		}

		void ControlOnLoaded(object sender, RoutedEventArgs e)
		{
			_scrollViewer = Control.FindVisualChild<ScrollViewer>();
			if (_scrollViewer != null)
			{
				_scrollViewer.ScrollChanged += SendScrolled;
			}
		}

		void SendScrolled(object sender, ScrollChangedEventArgs e)
		{
			var args = new ScrolledEventArgs(0, _scrollViewer.VerticalOffset);
			Element?.SendScrolled(args);
		}

		void OnElementScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			var scrollArgs = (ITemplatedItemsListScrollToRequestedEventArgs)e;
			ScrollTo(scrollArgs.Group, scrollArgs.Item, e.Position, e.ShouldAnimate);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ScrollView.VerticalScrollBarVisibilityProperty.PropertyName)
			{
				UpdateVerticalScrollBarVisibility();
			}
			else if (e.PropertyName == ScrollView.HorizontalScrollBarVisibilityProperty.PropertyName)
			{
				UpdateHorizontalScrollBarVisibility();
			}
		}

		void UpdateItemSource()
		{
			List<object> items = new List<object>();

			if (Element.IsGroupingEnabled)
			{
				int index = 0;
				foreach (var groupItem in TemplatedItemsView.TemplatedItems)
				{
					var group = TemplatedItemsView.TemplatedItems.GetGroup(index);

					if (group.Count != 0)
					{
						items.Add(group.HeaderContent);

						items.AddRange(group);
					}

					index++;
				}

				Control.ItemsSource = items;
			}
			else
			{
				foreach (var item in TemplatedItemsView.TemplatedItems)
				{
					items.Add(item);
				}

				Control.ItemsSource = items;
			}
		}

		void UpdateVerticalScrollBarVisibility()
		{
			if (_defaultVerticalScrollVisibility == null)
				_defaultVerticalScrollVisibility = ScrollViewer.GetVerticalScrollBarVisibility(Control);

			switch (Element.VerticalScrollBarVisibility)
			{
				case (ScrollBarVisibility.Always):
					ScrollViewer.SetVerticalScrollBarVisibility(Control, WpfScrollBarVisibility.Visible);
					break;
				case (ScrollBarVisibility.Never):
					ScrollViewer.SetVerticalScrollBarVisibility(Control, WpfScrollBarVisibility.Hidden);
					break;
				case (ScrollBarVisibility.Default):
					ScrollViewer.SetVerticalScrollBarVisibility(Control, (WpfScrollBarVisibility)_defaultVerticalScrollVisibility);
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
					ScrollViewer.SetHorizontalScrollBarVisibility(Control, WpfScrollBarVisibility.Visible);
					break;
				case (ScrollBarVisibility.Never):
					ScrollViewer.SetHorizontalScrollBarVisibility(Control, WpfScrollBarVisibility.Hidden);
					break;
				case (ScrollBarVisibility.Default):
					ScrollViewer.SetHorizontalScrollBarVisibility(Control, (WpfScrollBarVisibility)_defaultHorizontalScrollVisibility);
					break;
			}
		}

		void OnNativeKeyUp(object sender, KeyEventArgs e)
			=> Element.NotifyRowTapped(Control.SelectedIndex, cell: null);

		void OnNativeMouseUp(object sender, MouseButtonEventArgs e)
			=> Element.NotifyRowTapped(Control.SelectedIndex, cell: null);

		void OnNativeTouchUp(object sender, TouchEventArgs e)
			=> Element.NotifyRowTapped(Control.SelectedIndex, cell: null);

		void OnNativeStylusUp(object sender, StylusEventArgs e)
			=> Element.NotifyRowTapped(Control.SelectedIndex, cell: null);

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Control != null)
				{
					Control.MouseUp -= OnNativeMouseUp;
					Control.KeyUp -= OnNativeKeyUp;
					Control.TouchUp -= OnNativeTouchUp;
					Control.StylusUp -= OnNativeStylusUp;
					Control.Loaded -= ControlOnLoaded;
				}
				if (_scrollViewer != null)
				{
					_scrollViewer.ScrollChanged -= SendScrolled;
				}

				if (Element != null)
				{
					TemplatedItemsView.TemplatedItems.CollectionChanged -= OnCollectionChanged;
					TemplatedItemsView.TemplatedItems.GroupedCollectionChanged -= OnGroupedCollectionChanged;
				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}

		void OnElementItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if (Element == null)
				return;

			if (e.SelectedItem == null)
			{
				Control.SelectedIndex = -1;
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

			Control.SelectedIndex = index;
		}

		void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateItemSource();
		}

		void OnGroupedCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateItemSource();
		}
		void ScrollTo(object group, object item, ScrollToPosition toPosition, bool shouldAnimate)
		{
			var viewer = Control.FindVisualChild<ScrollViewer>();
			if (viewer == null)
			{
				RoutedEventHandler loadedHandler = null;
				loadedHandler = (o, e) =>
				{
					Control.Loaded -= loadedHandler;
					Device.BeginInvokeOnMainThread(() => { ScrollTo(group, item, toPosition, shouldAnimate); });
				};
				Control.Loaded += loadedHandler;
				return;
			}
			var templatedItems = TemplatedItemsView.TemplatedItems;
			Tuple<int, int> location = templatedItems.GetGroupAndIndexOfItem(group, item);
			if (location.Item1 == -1 || location.Item2 == -1)
				return;

			var t = templatedItems.GetGroup(location.Item1).ToArray();
			var c = t[location.Item2];

			Device.BeginInvokeOnMainThread(() =>
			{
				ScrollToPositionInView(Control, viewer, c, toPosition, shouldAnimate);
			});
		}
		static void ScrollToPositionInView(WList control, ScrollViewer sv, object item, ScrollToPosition position, bool animated)
		{
			// Scroll immediately if possible
			if (!TryScrollToPositionInView(control, sv, item, position, animated))
			{
				control.ScrollIntoView(item);
				control.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
				{
					TryScrollToPositionInView(control, sv, item, position, animated);
				}));
			}
		}

		static bool TryScrollToPositionInView(ItemsControl itemsControl, ScrollViewer sv, object item, ScrollToPosition position, bool animated)
		{
			var cell = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
			if (cell == null)
				return false;

			var cellHeight = cell.RenderSize.Height;
			var offsetInViewport = cell.TransformToAncestor(sv).Transform(new System.Windows.Point(0, 0)).Y;

			double newOffsetInViewport;
			switch (position)
			{
				case ScrollToPosition.Start:
					newOffsetInViewport = 0;
					break;
				case ScrollToPosition.Center:
					newOffsetInViewport = (sv.ViewportHeight - cellHeight) / 2;
					break;
				case ScrollToPosition.End:
					newOffsetInViewport = sv.ViewportHeight - cellHeight;
					break;
				case ScrollToPosition.MakeVisible:
					{
						var startOffset = 0;
						var endOffset = sv.ViewportHeight - cellHeight;
						var startDistance = Math.Abs(offsetInViewport - startOffset);
						var endDistance = Math.Abs(offsetInViewport - endOffset);
						// already in view, no action
						if (endOffset >= offsetInViewport && startOffset <= offsetInViewport)
							newOffsetInViewport = offsetInViewport;
						else if (startDistance < endDistance)
							newOffsetInViewport = startOffset;
						else
							newOffsetInViewport = endOffset;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(position), position, null);
			}

			if (newOffsetInViewport != offsetInViewport)
			{
				var offset = sv.VerticalOffset + offsetInViewport - newOffsetInViewport;
				ScrollToOffset(sv, offset, animated);
			}
			return true;
		}

		static void ScrollToOffset(ScrollViewer sv, double offset, bool animated)
		{
			if (sv.CanContentScroll)
			{
				var maxPossibleValue = sv.ExtentHeight - sv.ViewportHeight;
				offset = Math.Min(maxPossibleValue, Math.Max(0, offset));
				if (animated)
				{
					var animation = new DoubleAnimation
					{
						From = sv.VerticalOffset,
						To = offset,
						DecelerationRatio = 0.2,
						Duration = new Duration(TimeSpan.FromMilliseconds(200))
					};
					var storyboard = new Storyboard();
					storyboard.Children.Add(animation);
					Storyboard.SetTarget(animation, sv);
					Storyboard.SetTargetProperty(animation, new PropertyPath(ScrollViewerBehavior.VerticalOffsetProperty));
					storyboard.Begin();
				}
				else
				{
					sv.ScrollToVerticalOffset(offset);
				}
			}
		}
	}
}
