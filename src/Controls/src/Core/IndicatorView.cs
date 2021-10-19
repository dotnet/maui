using System;
using System.Collections;
using System.Collections.Specialized;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(IndicatorLayout))]
	public partial class IndicatorView : TemplatedView
	{
		const int DefaultPadding = 4;

		public static readonly BindableProperty IndicatorsShapeProperty = BindableProperty.Create(nameof(IndicatorsShape), typeof(IndicatorShape), typeof(IndicatorView), Controls.IndicatorShape.Circle);

		public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(int), typeof(IndicatorView), default(int), BindingMode.TwoWay);

		public static readonly BindableProperty CountProperty = BindableProperty.Create(nameof(Count), typeof(int), typeof(IndicatorView), default(int), propertyChanged: (bindable, oldValue, newValue)
			=> (((IndicatorView)bindable).IndicatorLayout as IndicatorStackLayout)?.ResetIndicatorCount((int)oldValue));

		public static readonly BindableProperty MaximumVisibleProperty = BindableProperty.Create(nameof(MaximumVisible), typeof(int), typeof(IndicatorView), int.MaxValue, propertyChanged: (bindable, oldValue, newValue)
		=> (((IndicatorView)bindable).IndicatorLayout as IndicatorStackLayout)?.ResetIndicators());

		public static readonly BindableProperty IndicatorTemplateProperty = BindableProperty.Create(nameof(IndicatorTemplate), typeof(DataTemplate), typeof(IndicatorView), default(DataTemplate), propertyChanging: (bindable, oldValue, newValue)
			=> UpdateIndicatorLayout((IndicatorView)bindable, newValue));

		public static readonly BindableProperty HideSingleProperty = BindableProperty.Create(nameof(HideSingle), typeof(bool), typeof(IndicatorView), true);

		public static readonly BindableProperty IndicatorColorProperty = BindableProperty.Create(nameof(IndicatorColor), typeof(Color), typeof(IndicatorView), Colors.LightGrey);

		public static readonly BindableProperty SelectedIndicatorColorProperty = BindableProperty.Create(nameof(SelectedIndicatorColor), typeof(Color), typeof(IndicatorView), Colors.Black);

		public static readonly BindableProperty IndicatorSizeProperty = BindableProperty.Create(nameof(IndicatorSize), typeof(double), typeof(IndicatorView), 6.0);

		public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(IndicatorView), null, propertyChanged: (bindable, oldValue, newValue)
			=> ((IndicatorView)bindable).ResetItemsSource((IEnumerable)oldValue));

		static readonly BindableProperty IndicatorLayoutProperty = BindableProperty.Create(nameof(IndicatorLayout), typeof(IBindableLayout), typeof(IndicatorView), null, propertyChanged: TemplateUtilities.OnContentChanged);

		public IndicatorView() { }

		public IndicatorShape IndicatorsShape
		{
			get { return (IndicatorShape)GetValue(IndicatorsShapeProperty); }
			set { SetValue(IndicatorsShapeProperty, value); }
		}

		public IBindableLayout IndicatorLayout
		{
			get => (IBindableLayout)GetValue(IndicatorLayoutProperty);
			set => SetValue(IndicatorLayoutProperty, value);
		}

		public int Position
		{
			get => (int)GetValue(PositionProperty);
			set => SetValue(PositionProperty, value);
		}

		public int Count
		{
			get => (int)GetValue(CountProperty);
			set => SetValue(CountProperty, value);
		}

		public int MaximumVisible
		{
			get => (int)GetValue(MaximumVisibleProperty);
			set => SetValue(MaximumVisibleProperty, value);
		}

		public DataTemplate IndicatorTemplate
		{
			get => (DataTemplate)GetValue(IndicatorTemplateProperty);
			set => SetValue(IndicatorTemplateProperty, value);
		}

		public bool HideSingle
		{
			get => (bool)GetValue(HideSingleProperty);
			set => SetValue(HideSingleProperty, value);
		}

		public Color IndicatorColor
		{
			get => (Color)GetValue(IndicatorColorProperty);
			set => SetValue(IndicatorColorProperty, value);
		}

		public Color SelectedIndicatorColor
		{
			get => (Color)GetValue(SelectedIndicatorColorProperty);
			set => SetValue(SelectedIndicatorColorProperty, value);
		}

		public double IndicatorSize
		{
			get => (double)GetValue(IndicatorSizeProperty);
			set => SetValue(IndicatorSizeProperty, value);
		}

		public IEnumerable ItemsSource
		{
			get => (IEnumerable)GetValue(ItemsSourceProperty);
			set => SetValue(ItemsSourceProperty, value);
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			if (IndicatorTemplate == null)
				return Device.PlatformServices.GetNativeSize(this, widthConstraint, heightConstraint);
			else
				return base.OnMeasure(widthConstraint, heightConstraint);
		}

		static void UpdateIndicatorLayout(IndicatorView indicatorView, object newValue)
		{
			if (newValue != null)
			{
				indicatorView.IndicatorLayout = new IndicatorStackLayout(indicatorView) { Spacing = DefaultPadding };
			}
			else if (indicatorView.IndicatorLayout == null)
			{
				(indicatorView.IndicatorLayout as IndicatorStackLayout)?.Remove();
				indicatorView.IndicatorLayout = null;
			}
		}

		void ResetItemsSource(IEnumerable oldItemsSource)
		{
			if (oldItemsSource is INotifyCollectionChanged oldCollection)
				oldCollection.CollectionChanged -= OnCollectionChanged;

			if (ItemsSource is INotifyCollectionChanged collection)
				collection.CollectionChanged += OnCollectionChanged;

			OnCollectionChanged(ItemsSource, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

			InvalidateMeasureInternal(Internals.InvalidationTrigger.MeasureChanged);
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (sender is ICollection collection)
			{
				Count = collection.Count;
				return;
			}
			var count = 0;
			var enumerator = (sender as IEnumerable)?.GetEnumerator();
			while (enumerator?.MoveNext() ?? false)
			{
				count++;
			}
			Count = count;
		}
	}
}