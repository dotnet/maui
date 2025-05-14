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
	/// <summary>
	/// A view that displays a visual indicator representing the position within a collection of items.
	/// </summary>
	/// <remarks>
	/// <see cref="IndicatorView"/> is commonly used with <see cref="CarouselView"/> to display dots or other shapes
	/// indicating the current position and total number of items. The indicators automatically update when
	/// the position changes.
	/// </remarks>
	[ContentProperty(nameof(IndicatorLayout))]
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler<IndicatorViewHandler>]
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

		/// <summary>
		/// Initializes a new instance of the <see cref="IndicatorView"/> class.
		/// </summary>
		public IndicatorView() { }

		/// <summary>
		/// Gets or sets the shape of the indicators.
		/// </summary>
		/// <value>An <see cref="IndicatorShape"/> value. The default is <see cref="IndicatorShape.Circle"/>.</value>
		public IndicatorShape IndicatorsShape
		{
			get { return (IndicatorShape)GetValue(IndicatorsShapeProperty); }
			set { SetValue(IndicatorsShapeProperty, value); }
		}

		/// <summary>
		/// Gets or sets the layout that arranges the indicator views.
		/// </summary>
		/// <value>An <see cref="IBindableLayout"/> that contains the indicator views.</value>
		/// <remarks>
		/// This property is set automatically when <see cref="IndicatorTemplate"/> is specified.
		/// Use this as the content property when defining custom indicator layouts in XAML.
		/// </remarks>
		public IBindableLayout IndicatorLayout
		{
			get => (IBindableLayout)GetValue(IndicatorLayoutProperty);
			set => SetValue(IndicatorLayoutProperty, value);
		}

		/// <summary>
		/// Gets or sets the index of the currently selected indicator.
		/// </summary>
		/// <value>The zero-based index of the current position. The default is 0.</value>
		/// <remarks>
		/// This property is typically bound to a <see cref="CarouselView.Position"/> property to
		/// keep the indicator view synchronized with the carousel position.
		/// </remarks>
		public int Position
		{
			get => (int)GetValue(PositionProperty);
			set => SetValue(PositionProperty, value);
		}

		/// <summary>
		/// Gets or sets the total number of indicators to display.
		/// </summary>
		/// <value>The total number of indicators. The default is 0.</value>
		/// <remarks>
		/// This is automatically set when <see cref="ItemsSource"/> is provided.
		/// You can also set this manually when not using an items source.
		/// </remarks>
		public int Count
		{
			get => (int)GetValue(CountProperty);
			set => SetValue(CountProperty, value);
		}

		/// <summary>
		/// Gets or sets the maximum number of indicators to display at once.
		/// </summary>
		/// <value>The maximum number of visible indicators. The default is <see cref="int.MaxValue"/>.</value>
		/// <remarks>
		/// When the total count exceeds this value, only the most relevant indicators around
		/// the current position are shown. Useful for large collections to avoid cluttering the UI.
		/// </remarks>
		public int MaximumVisible
		{
			get => (int)GetValue(MaximumVisibleProperty);
			set => SetValue(MaximumVisibleProperty, value);
		}

		/// <summary>
		/// Gets or sets the data template used to display each indicator.
		/// </summary>
		/// <value>A <see cref="DataTemplate"/> that defines the appearance of each indicator, or <see langword="null"/> to use the default indicator style.</value>
		/// <remarks>
		/// When set, this allows full customization of indicator appearance. When <see langword="null"/>,
		/// indicators are rendered using <see cref="IndicatorsShape"/>, <see cref="IndicatorSize"/>, and color properties.
		/// </remarks>
		public DataTemplate IndicatorTemplate
		{
			get => (DataTemplate)GetValue(IndicatorTemplateProperty);
			set => SetValue(IndicatorTemplateProperty, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the indicator view is hidden when only one item exists.
		/// </summary>
		/// <value><see langword="true"/> to hide the indicator view when there is only one item; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
		/// <remarks>
		/// When enabled and <see cref="Count"/> is 1 or less, the indicator view will not be displayed.
		/// </remarks>
		public bool HideSingle
		{
			get => (bool)GetValue(HideSingleProperty);
			set => SetValue(HideSingleProperty, value);
		}

		/// <summary>
		/// Gets or sets the color of unselected indicators.
		/// </summary>
		/// <value>A <see cref="Color"/> for unselected indicators. The default is <see cref="Colors.LightGrey"/>.</value>
		public Color IndicatorColor
		{
			get => (Color)GetValue(IndicatorColorProperty);
			set => SetValue(IndicatorColorProperty, value);
		}

		/// <summary>
		/// Gets or sets the color of the selected indicator.
		/// </summary>
		/// <value>A <see cref="Color"/> for the selected indicator. The default is <see cref="Colors.Black"/>.</value>
		public Color SelectedIndicatorColor
		{
			get => (Color)GetValue(SelectedIndicatorColorProperty);
			set => SetValue(SelectedIndicatorColorProperty, value);
		}

		/// <summary>
		/// Gets or sets the size of each indicator.
		/// </summary>
		/// <value>The size of each indicator in device-independent units. The default is 6.0.</value>
		/// <remarks>
		/// This value determines both the width and height of each indicator when using the default rendering.
		/// </remarks>
		public double IndicatorSize
		{
			get => (double)GetValue(IndicatorSizeProperty);
			set => SetValue(IndicatorSizeProperty, value);
		}

		/// <summary>
		/// Gets or sets the collection of items for which indicators will be displayed.
		/// </summary>
		/// <value>An <see cref="IEnumerable"/> collection of items, or <see langword="null"/>.</value>
		/// <remarks>
		/// When set, the <see cref="Count"/> property is automatically updated based on the number of items.
		/// Typically bound to the same collection as a <see cref="CarouselView"/>'s ItemsSource property.
		/// </remarks>
		public IEnumerable ItemsSource
		{
			get => (IEnumerable)GetValue(ItemsSourceProperty);
			set => SetValue(ItemsSourceProperty, value);
		}

		/// <summary>
		/// Measures the indicator view to determine its desired size.
		/// </summary>
		/// <param name="widthConstraint">The available width.</param>
		/// <param name="heightConstraint">The available height.</param>
		/// <returns>A <see cref="SizeRequest"/> indicating the desired size of the indicator view.</returns>
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