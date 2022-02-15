using System;
using System.Collections;
using System.Collections.Specialized;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="Type[@FullName='Microsoft.Maui.Controls.IndicatorView']/Docs" />
	[ContentProperty(nameof(IndicatorLayout))]
	public partial class IndicatorView : TemplatedView
	{
		const int DefaultPadding = 4;

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='IndicatorsShapeProperty']/Docs" />
		public static readonly BindableProperty IndicatorsShapeProperty = BindableProperty.Create(nameof(IndicatorsShape), typeof(IndicatorShape), typeof(IndicatorView), Controls.IndicatorShape.Circle);

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='PositionProperty']/Docs" />
		public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(int), typeof(IndicatorView), default(int), BindingMode.TwoWay);

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='CountProperty']/Docs" />
		public static readonly BindableProperty CountProperty = BindableProperty.Create(nameof(Count), typeof(int), typeof(IndicatorView), default(int), propertyChanged: (bindable, oldValue, newValue)
			=> (((IndicatorView)bindable).IndicatorLayout as IndicatorStackLayout)?.ResetIndicatorCount((int)oldValue));

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='MaximumVisibleProperty']/Docs" />
		public static readonly BindableProperty MaximumVisibleProperty = BindableProperty.Create(nameof(MaximumVisible), typeof(int), typeof(IndicatorView), int.MaxValue, propertyChanged: (bindable, oldValue, newValue)
		=> (((IndicatorView)bindable).IndicatorLayout as IndicatorStackLayout)?.ResetIndicators());

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='IndicatorTemplateProperty']/Docs" />
		public static readonly BindableProperty IndicatorTemplateProperty = BindableProperty.Create(nameof(IndicatorTemplate), typeof(DataTemplate), typeof(IndicatorView), default(DataTemplate), propertyChanging: (bindable, oldValue, newValue)
			=> UpdateIndicatorLayout((IndicatorView)bindable, newValue));

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='HideSingleProperty']/Docs" />
		public static readonly BindableProperty HideSingleProperty = BindableProperty.Create(nameof(HideSingle), typeof(bool), typeof(IndicatorView), true);

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='IndicatorColorProperty']/Docs" />
		public static readonly BindableProperty IndicatorColorProperty = BindableProperty.Create(nameof(IndicatorColor), typeof(Color), typeof(IndicatorView), Colors.LightGrey);

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='SelectedIndicatorColorProperty']/Docs" />
		public static readonly BindableProperty SelectedIndicatorColorProperty = BindableProperty.Create(nameof(SelectedIndicatorColor), typeof(Color), typeof(IndicatorView), Colors.Black);

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='IndicatorSizeProperty']/Docs" />
		public static readonly BindableProperty IndicatorSizeProperty = BindableProperty.Create(nameof(IndicatorSize), typeof(double), typeof(IndicatorView), 6.0);

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='ItemsSourceProperty']/Docs" />
		public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(IndicatorView), null, propertyChanged: (bindable, oldValue, newValue)
			=> ((IndicatorView)bindable).ResetItemsSource((IEnumerable)oldValue));

		static readonly BindableProperty IndicatorLayoutProperty = BindableProperty.Create(nameof(IndicatorLayout), typeof(IBindableLayout), typeof(IndicatorView), null, propertyChanged: TemplateUtilities.OnContentChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public IndicatorView() { }

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='IndicatorsShape']/Docs" />
		public IndicatorShape IndicatorsShape
		{
			get { return (IndicatorShape)GetValue(IndicatorsShapeProperty); }
			set { SetValue(IndicatorsShapeProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='IndicatorLayout']/Docs" />
		public IBindableLayout IndicatorLayout
		{
			get => (IBindableLayout)GetValue(IndicatorLayoutProperty);
			set => SetValue(IndicatorLayoutProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='Position']/Docs" />
		public int Position
		{
			get => (int)GetValue(PositionProperty);
			set => SetValue(PositionProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='Count']/Docs" />
		public int Count
		{
			get => (int)GetValue(CountProperty);
			set => SetValue(CountProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='MaximumVisible']/Docs" />
		public int MaximumVisible
		{
			get => (int)GetValue(MaximumVisibleProperty);
			set => SetValue(MaximumVisibleProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='IndicatorTemplate']/Docs" />
		public DataTemplate IndicatorTemplate
		{
			get => (DataTemplate)GetValue(IndicatorTemplateProperty);
			set => SetValue(IndicatorTemplateProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='HideSingle']/Docs" />
		public bool HideSingle
		{
			get => (bool)GetValue(HideSingleProperty);
			set => SetValue(HideSingleProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='IndicatorColor']/Docs" />
		public Color IndicatorColor
		{
			get => (Color)GetValue(IndicatorColorProperty);
			set => SetValue(IndicatorColorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='SelectedIndicatorColor']/Docs" />
		public Color SelectedIndicatorColor
		{
			get => (Color)GetValue(SelectedIndicatorColorProperty);
			set => SetValue(SelectedIndicatorColorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='IndicatorSize']/Docs" />
		public double IndicatorSize
		{
			get => (double)GetValue(IndicatorSizeProperty);
			set => SetValue(IndicatorSizeProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='ItemsSource']/Docs" />
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