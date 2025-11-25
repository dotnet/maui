#nullable disable
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Handlers;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="Type[@FullName='Microsoft.Maui.Controls.IndicatorView']/Docs/*" />
	[ContentProperty(nameof(IndicatorLayout))]
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler(typeof(IndicatorViewHandler))]
	public partial class IndicatorView : TemplatedView, ITemplatedIndicatorView
	{
		const int DefaultPadding = 4;

		/// <summary>Bindable property for <see cref="IndicatorsShape"/>.</summary>
		public static readonly BindableProperty IndicatorsShapeProperty = BindableProperty.Create(nameof(IndicatorsShape), typeof(IndicatorShape), typeof(IndicatorView), Controls.IndicatorShape.Circle);

		/// <summary>Bindable property for <see cref="Position"/>.</summary>
		public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(int), typeof(IndicatorView), default(int), BindingMode.TwoWay);

		/// <summary>Bindable property for <see cref="Count"/>.</summary>
		public static readonly BindableProperty CountProperty = BindableProperty.Create(nameof(Count), typeof(int), typeof(IndicatorView), default(int), propertyChanged: (bindable, oldValue, newValue)
			=> (((IndicatorView)bindable).IndicatorLayout as IndicatorStackLayout)?.ResetIndicatorCount((int)oldValue));

		/// <summary>Bindable property for <see cref="MaximumVisible"/>.</summary>
		public static readonly BindableProperty MaximumVisibleProperty = BindableProperty.Create(nameof(MaximumVisible), typeof(int), typeof(IndicatorView), int.MaxValue, propertyChanged: (bindable, oldValue, newValue)
		=> (((IndicatorView)bindable).IndicatorLayout as IndicatorStackLayout)?.ResetIndicators());

		/// <summary>Bindable property for <see cref="IndicatorTemplate"/>.</summary>
		public static readonly BindableProperty IndicatorTemplateProperty = BindableProperty.Create(nameof(IndicatorTemplate), typeof(DataTemplate), typeof(IndicatorView), default(DataTemplate), propertyChanging: (bindable, oldValue, newValue)
			=> UpdateIndicatorLayout((IndicatorView)bindable, newValue));

		/// <summary>Bindable property for <see cref="HideSingle"/>.</summary>
		public static readonly BindableProperty HideSingleProperty = BindableProperty.Create(nameof(HideSingle), typeof(bool), typeof(IndicatorView), true);

		/// <summary>Bindable property for <see cref="IndicatorColor"/>.</summary>
		public static readonly BindableProperty IndicatorColorProperty = BindableProperty.Create(nameof(IndicatorColor), typeof(Color), typeof(IndicatorView), Colors.LightGrey);

		/// <summary>Bindable property for <see cref="SelectedIndicatorColor"/>.</summary>
		public static readonly BindableProperty SelectedIndicatorColorProperty = BindableProperty.Create(nameof(SelectedIndicatorColor), typeof(Color), typeof(IndicatorView), Colors.Black);

		/// <summary>Bindable property for <see cref="IndicatorSize"/>.</summary>
		public static readonly BindableProperty IndicatorSizeProperty = BindableProperty.Create(nameof(IndicatorSize), typeof(double), typeof(IndicatorView), 6.0);

		/// <summary>Bindable property for <see cref="ItemsSource"/>.</summary>
		public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(IndicatorView), null, propertyChanged: (bindable, oldValue, newValue)
			=> ((IndicatorView)bindable).ResetItemsSource((IEnumerable)oldValue));

		static readonly BindableProperty IndicatorLayoutProperty = BindableProperty.Create(nameof(IndicatorLayout), typeof(IBindableLayout), typeof(IndicatorView), null, propertyChanged: TemplateUtilities.OnContentChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public IndicatorView() { }

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='IndicatorsShape']/Docs/*" />
		public IndicatorShape IndicatorsShape
		{
			get { return (IndicatorShape)GetValue(IndicatorsShapeProperty); }
			set { SetValue(IndicatorsShapeProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='IndicatorLayout']/Docs/*" />
		public IBindableLayout IndicatorLayout
		{
			get => (IBindableLayout)GetValue(IndicatorLayoutProperty);
			set => SetValue(IndicatorLayoutProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='Position']/Docs/*" />
		public int Position
		{
			get => (int)GetValue(PositionProperty);
			set => SetValue(PositionProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='Count']/Docs/*" />
		public int Count
		{
			get => (int)GetValue(CountProperty);
			set => SetValue(CountProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='MaximumVisible']/Docs/*" />
		public int MaximumVisible
		{
			get => (int)GetValue(MaximumVisibleProperty);
			set => SetValue(MaximumVisibleProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='IndicatorTemplate']/Docs/*" />
		public DataTemplate IndicatorTemplate
		{
			get => (DataTemplate)GetValue(IndicatorTemplateProperty);
			set => SetValue(IndicatorTemplateProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='HideSingle']/Docs/*" />
		public bool HideSingle
		{
			get => (bool)GetValue(HideSingleProperty);
			set => SetValue(HideSingleProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='IndicatorColor']/Docs/*" />
		public Color IndicatorColor
		{
			get => (Color)GetValue(IndicatorColorProperty);
			set => SetValue(IndicatorColorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='SelectedIndicatorColor']/Docs/*" />
		public Color SelectedIndicatorColor
		{
			get => (Color)GetValue(SelectedIndicatorColorProperty);
			set => SetValue(SelectedIndicatorColorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='IndicatorSize']/Docs/*" />
		public double IndicatorSize
		{
			get => (double)GetValue(IndicatorSizeProperty);
			set => SetValue(IndicatorSizeProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/IndicatorView.xml" path="//Member[@MemberName='ItemsSource']/Docs/*" />
		public IEnumerable ItemsSource
		{
			get => (IEnumerable)GetValue(ItemsSourceProperty);
			set => SetValue(ItemsSourceProperty, value);
		}

		[Obsolete("Use MeasureOverride instead")]
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			if (IndicatorTemplate == null)
			{
				return Handler?.GetDesiredSize(widthConstraint, heightConstraint) ?? new();
			}

			return base.OnMeasure(widthConstraint, heightConstraint);
		}

		static void UpdateIndicatorLayout(IndicatorView indicatorView, object newValue)
		{
			if (newValue != null)
			{
				indicatorView.IndicatorLayout = new IndicatorStackLayout(indicatorView) { Spacing = DefaultPadding };
			}
			else if (indicatorView.IndicatorLayout is not null)
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

		Paint IIndicatorView.IndicatorColor => IndicatorColor?.AsPaint();
		Paint IIndicatorView.SelectedIndicatorColor => SelectedIndicatorColor?.AsPaint();
		IShape IIndicatorView.IndicatorsShape => IndicatorsShape == IndicatorShape.Square ? new Shapes.Rectangle() : new Shapes.Ellipse();
		Maui.ILayout ITemplatedIndicatorView.IndicatorsLayoutOverride => (IndicatorTemplate != null) ? IndicatorLayout as Maui.ILayout : null;

		int IIndicatorView.Position
		{
			get => Position;
			set => SetValue(PositionProperty, value, SetterSpecificity.FromHandler);
		}

		private protected override string GetDebuggerDisplay()
		{
			var debugText = DebuggerDisplayHelpers.GetDebugText(nameof(Position), Position, nameof(Count), Count);
			return $"{base.GetDebuggerDisplay()}, {debugText}";
		}
	}
}