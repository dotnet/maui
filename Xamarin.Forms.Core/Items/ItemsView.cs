using System;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Xamarin.Forms
{
	public class ItemsView : View
	{
		protected internal ItemsView()
		{
			CollectionView.VerifyCollectionViewFlagEnabled(constructorHint: nameof(ItemsView));
		}

		public static readonly BindableProperty EmptyViewProperty =
			BindableProperty.Create(nameof(EmptyView), typeof(object), typeof(ItemsView), null);

		public object EmptyView
		{
			get => GetValue(EmptyViewProperty);
			set => SetValue(EmptyViewProperty, value);
		}

		public static readonly BindableProperty EmptyViewTemplateProperty =
			BindableProperty.Create(nameof(EmptyViewTemplate), typeof(DataTemplate), typeof(ItemsView), null);

		public DataTemplate EmptyViewTemplate
		{
			get => (DataTemplate)GetValue(EmptyViewTemplateProperty);
			set => SetValue(EmptyViewTemplateProperty, value);
		}

		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(ItemsView), null);

		public IEnumerable ItemsSource 
		{
			get => (IEnumerable)GetValue(ItemsSourceProperty);
			set => SetValue(ItemsSourceProperty, value);
		}

		// TODO hartez 2018/08/29 17:35:10 Should ItemsView be abstract? With ItemsLayout as an interface?
		// Trying to come up with a reasonable way to restrict CarouselView to ListItemsLayout(LinearLayout) 
		// ((because setting Carousel to grid is ... weird? And by default it just won't do anything.))
		// And allow CollectionView to use a broader set of Layout options
		// So the Bindable property only exists at the CarouselView/CollectionView (i.e., concrete class) level
		// but some version of IItemsLayout is still here?

		public static readonly BindableProperty ItemsLayoutProperty =
			BindableProperty.Create(nameof(ItemsLayout), typeof(IItemsLayout), typeof(ItemsView), 
				ListItemsLayout.VerticalList);

		public IItemsLayout ItemsLayout
		{
			get => (IItemsLayout)GetValue(ItemsLayoutProperty);
			set => SetValue(ItemsLayoutProperty, value);
		}

		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(ItemsView));

		public DataTemplate ItemTemplate
		{
			get => (DataTemplate)GetValue(ItemTemplateProperty);
			set => SetValue(ItemTemplateProperty, value);
		}

		public void ScrollTo(int index, int groupIndex = -1,
			ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
		{
			OnScrollToRequested(new ScrollToRequestEventArgs(index, groupIndex, position, animate));
		}

		public void ScrollTo(object item, object group = null,
			ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
		{
			OnScrollToRequested(new ScrollToRequestEventArgs(item, group, position, animate));
		}

		public event EventHandler<ScrollToRequestEventArgs> ScrollToRequested;

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			// TODO hartez 2018-05-22 05:04 PM This 40,40 is what LV1 does; can we come up with something less arbitrary?
			var minimumSize = new Size(40, 40);

			var maxWidth = Math.Min(Device.Info.ScaledScreenSize.Width, widthConstraint);
			var maxHeight = Math.Min(Device.Info.ScaledScreenSize.Height, heightConstraint);

			Size request = new Size(maxWidth, maxHeight);

			return new SizeRequest(request, minimumSize);
		}

		protected virtual void OnScrollToRequested(ScrollToRequestEventArgs e)
		{
			ScrollToRequested?.Invoke(this, e);
		}
	}
}