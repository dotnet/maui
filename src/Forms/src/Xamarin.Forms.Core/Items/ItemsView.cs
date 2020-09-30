using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml.Diagnostics;

namespace Xamarin.Forms
{
	public abstract class ItemsView : View
	{
		List<Element> _logicalChildren = new List<Element>();

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

		public static readonly BindableProperty RemainingItemsThresholdReachedCommandProperty =
			BindableProperty.Create(nameof(RemainingItemsThresholdReachedCommand), typeof(ICommand), typeof(ItemsView), null);

		public ICommand RemainingItemsThresholdReachedCommand
		{
			get => (ICommand)GetValue(RemainingItemsThresholdReachedCommandProperty);
			set => SetValue(RemainingItemsThresholdReachedCommandProperty, value);
		}

		public static readonly BindableProperty RemainingItemsThresholdReachedCommandParameterProperty = BindableProperty.Create(nameof(RemainingItemsThresholdReachedCommandParameter), typeof(object), typeof(ItemsView), default(object));

		public object RemainingItemsThresholdReachedCommandParameter
		{
			get => GetValue(RemainingItemsThresholdReachedCommandParameterProperty);
			set => SetValue(RemainingItemsThresholdReachedCommandParameterProperty, value);
		}

		public static readonly BindableProperty HorizontalScrollBarVisibilityProperty = BindableProperty.Create(
			nameof(HorizontalScrollBarVisibility),
			typeof(ScrollBarVisibility),
			typeof(ItemsView),
			ScrollBarVisibility.Default);

		public ScrollBarVisibility HorizontalScrollBarVisibility
		{
			get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
			set => SetValue(HorizontalScrollBarVisibilityProperty, value);
		}

		public static readonly BindableProperty VerticalScrollBarVisibilityProperty = BindableProperty.Create(
			nameof(VerticalScrollBarVisibility),
			typeof(ScrollBarVisibility),
			typeof(ItemsView),
			ScrollBarVisibility.Default);

		public ScrollBarVisibility VerticalScrollBarVisibility
		{
			get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
			set => SetValue(VerticalScrollBarVisibilityProperty, value);
		}

		public static readonly BindableProperty RemainingItemsThresholdProperty =
			BindableProperty.Create(nameof(RemainingItemsThreshold), typeof(int), typeof(ItemsView), -1, validateValue: (bindable, value) => (int)value >= -1);

		public int RemainingItemsThreshold
		{
			get => (int)GetValue(RemainingItemsThresholdProperty);
			set => SetValue(RemainingItemsThresholdProperty, value);
		}

		public void AddLogicalChild(Element element)
		{
			if (element == null)
			{
				return;
			}

			_logicalChildren.Add(element);
			element.Parent = this;
			OnChildAdded(element);
			VisualDiagnostics.OnChildAdded(this, element);
		}

		public void RemoveLogicalChild(Element element)
		{
			if (element == null)
			{
				return;
			}

			element.Parent = null;

			if (!_logicalChildren.Contains(element))
				return;

			var oldLogicalIndex = _logicalChildren.IndexOf(element);
			_logicalChildren.Remove(element);
			OnChildRemoved(element, oldLogicalIndex);
			VisualDiagnostics.OnChildRemoved(this, element, oldLogicalIndex);
		}

#if NETSTANDARD1_0
		ReadOnlyCollection<Element> _readOnlyLogicalChildren;
		internal override ReadOnlyCollection<Element> LogicalChildrenInternal => _readOnlyLogicalChildren ?? 
			(_readOnlyLogicalChildren = new ReadOnlyCollection<Element>(_logicalChildren));
#else
		internal override ReadOnlyCollection<Element> LogicalChildrenInternal => _logicalChildren.AsReadOnly();
#endif

		internal static readonly BindableProperty InternalItemsLayoutProperty =
			BindableProperty.Create(nameof(ItemsLayout), typeof(IItemsLayout), typeof(ItemsView),
				LinearItemsLayout.Vertical, propertyChanged: OnInternalItemsLayoutPropertyChanged);

		static void OnInternalItemsLayoutPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue is BindableObject boOld)
				SetInheritedBindingContext(boOld, null);

			if (newValue is BindableObject boNew)
				SetInheritedBindingContext(boNew, bindable.BindingContext);
		}

		protected IItemsLayout InternalItemsLayout
		{
			get => (IItemsLayout)GetValue(InternalItemsLayoutProperty);
			set => SetValue(InternalItemsLayoutProperty, value);
		}

		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(ItemsView));

		public DataTemplate ItemTemplate
		{
			get => (DataTemplate)GetValue(ItemTemplateProperty);
			set => SetValue(ItemTemplateProperty, value);
		}

		public static readonly BindableProperty ItemsUpdatingScrollModeProperty =
			BindableProperty.Create(nameof(ItemsUpdatingScrollMode), typeof(ItemsUpdatingScrollMode), typeof(ItemsView),
				default(ItemsUpdatingScrollMode));

		public ItemsUpdatingScrollMode ItemsUpdatingScrollMode
		{
			get => (ItemsUpdatingScrollMode)GetValue(ItemsUpdatingScrollModeProperty);
			set => SetValue(ItemsUpdatingScrollModeProperty, value);
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

		public void SendRemainingItemsThresholdReached()
		{
			RemainingItemsThresholdReached?.Invoke(this, EventArgs.Empty);

			if (RemainingItemsThresholdReachedCommand?.CanExecute(RemainingItemsThresholdReachedCommandParameter) == true)
				RemainingItemsThresholdReachedCommand?.Execute(RemainingItemsThresholdReachedCommandParameter);

			OnRemainingItemsThresholdReached();
		}

		public void SendScrolled(ItemsViewScrolledEventArgs e)
		{
			Scrolled?.Invoke(this, e);

			OnScrolled(e);
		}

		public event EventHandler<ScrollToRequestEventArgs> ScrollToRequested;

		public event EventHandler<ItemsViewScrolledEventArgs> Scrolled;

		public event EventHandler RemainingItemsThresholdReached;

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

		protected virtual void OnRemainingItemsThresholdReached()
		{

		}

		protected virtual void OnScrolled(ItemsViewScrolledEventArgs e)
		{

		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			if (InternalItemsLayout is BindableObject bo)
				SetInheritedBindingContext(bo, BindingContext);
		}
	}
}