#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A view that presents a scrollable collection of items where each item 'snaps' into place after scrolling.
	/// </summary>
	/// <remarks>
	/// <see cref="CarouselView"/> is useful for displaying a horizontal or vertical carousel of items,
	/// where the user can swipe through items and each item snaps into view. Unlike <see cref="CollectionView"/>,
	/// <see cref="CarouselView"/> enforces single-item snap points and provides additional features like looping
	/// and position tracking.
	/// </remarks>
	[ElementHandler(typeof(Handlers.Items2.CarouselViewHandler2))]
	public class CarouselView : ItemsView
	{
		/// <summary>
		/// Visual state name for the current item in the carousel.
		/// </summary>
		/// <value>The string "CurrentItem".</value>
		public const string CurrentItemVisualState = "CurrentItem";

		/// <summary>
		/// Visual state name for the next item in the carousel.
		/// </summary>
		/// <value>The string "NextItem".</value>
		public const string NextItemVisualState = "NextItem";

		/// <summary>
		/// Visual state name for the previous item in the carousel.
		/// </summary>
		/// <value>The string "PreviousItem".</value>
		public const string PreviousItemVisualState = "PreviousItem";

		/// <summary>
		/// Visual state name for items that are neither current, next, nor previous.
		/// </summary>
		/// <value>The string "DefaultItem".</value>
		public const string DefaultItemVisualState = "DefaultItem";

		/// <summary>Bindable property for <see cref="Loop"/>.</summary>
		public static readonly BindableProperty LoopProperty = BindableProperty.Create(nameof(Loop), typeof(bool), typeof(CarouselView), true, BindingMode.OneTime);

		/// <summary>
		/// Gets or sets a value indicating whether the carousel loops back to the first item after reaching the last item.
		/// </summary>
		/// <value><see langword="true"/> if the carousel should loop continuously; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
		/// <remarks>
		/// When enabled, scrolling past the last item returns to the first item, and scrolling before the first item goes to the last item.
		/// </remarks>
		public bool Loop
		{
			get { return (bool)GetValue(LoopProperty); }
			set { SetValue(LoopProperty, value); }
		}

		/// <summary>Bindable property for <see cref="PeekAreaInsets"/>.</summary>
		public static readonly BindableProperty PeekAreaInsetsProperty = BindableProperty.Create(nameof(PeekAreaInsets), typeof(Thickness), typeof(CarouselView), default(Thickness));

		/// <summary>
		/// Gets or sets the amount of space to reserve on each side of the current item to show a peek of adjacent items.
		/// </summary>
		/// <value>A <see cref="Thickness"/> value defining the peek area insets. The default is 0 on all sides.</value>
		/// <remarks>
		/// Use this property to show a preview of adjacent items on either side of the current item.
		/// This helps users understand that more items are available by swiping.
		/// </remarks>
		public Thickness PeekAreaInsets
		{
			get { return (Thickness)GetValue(PeekAreaInsetsProperty); }
			set { SetValue(PeekAreaInsetsProperty, value); }
		}

		static readonly BindablePropertyKey VisibleViewsPropertyKey = BindableProperty.CreateReadOnly(nameof(VisibleViews), typeof(ObservableCollection<View>), typeof(CarouselView), null, defaultValueCreator: (b) => new ObservableCollection<View>());

		/// <summary>Bindable property for <see cref="VisibleViews"/>.</summary>
		public static readonly BindableProperty VisibleViewsProperty = VisibleViewsPropertyKey.BindableProperty;

		/// <summary>
		/// Gets the collection of views currently visible in the carousel.
		/// </summary>
		/// <value>An <see cref="ObservableCollection{T}"/> of <see cref="View"/> objects that are currently visible.</value>
		/// <remarks>
		/// This collection is automatically updated as the user scrolls through the carousel.
		/// It includes the current item and any partially visible adjacent items based on <see cref="PeekAreaInsets"/>.
		/// </remarks>
		public ObservableCollection<View> VisibleViews => (ObservableCollection<View>)GetValue(VisibleViewsProperty);

		static readonly BindablePropertyKey IsDraggingPropertyKey = BindableProperty.CreateReadOnly(nameof(IsDragging), typeof(bool), typeof(CarouselView), false);

		/// <summary>Bindable property for <see cref="IsDragging"/>.</summary>
		public static readonly BindableProperty IsDraggingProperty = IsDraggingPropertyKey.BindableProperty;

		/// <summary>
		/// Gets a value indicating whether the user is currently dragging the carousel.
		/// </summary>
		/// <value><see langword="true"/> if the user is actively dragging; otherwise, <see langword="false"/>.</value>
		/// <remarks>
		/// This property is set to <see langword="true"/> when the user begins a drag gesture and returns to <see langword="false"/>
		/// when the gesture completes or is cancelled.
		/// </remarks>
		public bool IsDragging => (bool)GetValue(IsDraggingProperty);

		/// <summary>Bindable property for <see cref="IsBounceEnabled"/>.</summary>
		public static readonly BindableProperty IsBounceEnabledProperty =
			BindableProperty.Create(nameof(IsBounceEnabled), typeof(bool), typeof(CarouselView), true);

		/// <summary>
		/// Gets or sets a value indicating whether bounce effects are enabled when scrolling reaches the end of the carousel.
		/// </summary>
		/// <value><see langword="true"/> to enable bounce effects; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
		/// <remarks>
		/// On iOS, this controls whether the carousel bounces when reaching the first or last item.
		/// The effect may vary by platform.
		/// </remarks>
		public bool IsBounceEnabled
		{
			get { return (bool)GetValue(IsBounceEnabledProperty); }
			set { SetValue(IsBounceEnabledProperty, value); }
		}

		/// <summary>Bindable property for <see cref="IsSwipeEnabled"/>.</summary>
		public static readonly BindableProperty IsSwipeEnabledProperty =
			BindableProperty.Create(nameof(IsSwipeEnabled), typeof(bool), typeof(CarouselView), true);

		/// <summary>
		/// Gets or sets a value indicating whether swipe gestures are enabled for navigation.
		/// </summary>
		/// <value><see langword="true"/> to enable swipe gestures; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
		/// <remarks>
		/// When disabled, users cannot navigate between items using swipe gestures.
		/// Programmatic navigation using <see cref="Position"/> or <see cref="CurrentItem"/> still works when this is disabled.
		/// </remarks>
		public bool IsSwipeEnabled
		{
			get { return (bool)GetValue(IsSwipeEnabledProperty); }
			set { SetValue(IsSwipeEnabledProperty, value); }
		}

		/// <summary>Bindable property for <see cref="IsScrollAnimated"/>.</summary>
		public static readonly BindableProperty IsScrollAnimatedProperty =
		BindableProperty.Create(nameof(IsScrollAnimated), typeof(bool), typeof(CarouselView), true);

		/// <summary>
		/// Gets or sets a value indicating whether scrolling between items is animated.
		/// </summary>
		/// <value><see langword="true"/> to animate scrolling; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
		/// <remarks>
		/// When <see langword="true"/>, programmatic changes to <see cref="Position"/> or <see cref="CurrentItem"/>
		/// will animate the transition. When <see langword="false"/>, the carousel jumps instantly to the new position.
		/// </remarks>
		public bool IsScrollAnimated
		{
			get { return (bool)GetValue(IsScrollAnimatedProperty); }
			set { SetValue(IsScrollAnimatedProperty, value); }
		}

		/// <summary>Bindable property for <see cref="CurrentItem"/>.</summary>
		public static readonly BindableProperty CurrentItemProperty =
		BindableProperty.Create(nameof(CurrentItem), typeof(object), typeof(CarouselView), default, BindingMode.TwoWay,
			propertyChanged: CurrentItemPropertyChanged);

		/// <summary>Bindable property for <see cref="CurrentItemChangedCommand"/>.</summary>
		public static readonly BindableProperty CurrentItemChangedCommandProperty =
			BindableProperty.Create(nameof(CurrentItemChangedCommand), typeof(ICommand), typeof(CarouselView));

		/// <summary>Bindable property for <see cref="CurrentItemChangedCommandParameter"/>.</summary>
		public static readonly BindableProperty CurrentItemChangedCommandParameterProperty =
			BindableProperty.Create(nameof(CurrentItemChangedCommandParameter), typeof(object), typeof(CarouselView));

		/// <summary>
		/// Gets or sets the currently displayed item in the carousel.
		/// </summary>
		/// <value>The data item currently centered in the carousel, or <see langword="null"/> if no item is selected.</value>
		/// <remarks>
		/// This property is bindable and supports two-way binding. Setting this property programmatically
		/// will scroll the carousel to display the specified item.
		/// </remarks>
		public object CurrentItem
		{
			get => GetValue(CurrentItemProperty);
			set => SetValue(CurrentItemProperty, value);
		}

		/// <summary>
		/// Gets or sets the command to execute when the current item changes.
		/// </summary>
		/// <value>An <see cref="ICommand"/> to execute when the current item changes.</value>
		public ICommand CurrentItemChangedCommand
		{
			get => (ICommand)GetValue(CurrentItemChangedCommandProperty);
			set => SetValue(CurrentItemChangedCommandProperty, value);
		}

		/// <summary>
		/// Gets or sets the parameter to pass to <see cref="CurrentItemChangedCommand"/>.
		/// </summary>
		/// <value>The parameter object to pass to the command.</value>
		public object CurrentItemChangedCommandParameter
		{
			get => GetValue(CurrentItemChangedCommandParameterProperty);
			set => SetValue(CurrentItemChangedCommandParameterProperty, value);
		}

		static void CurrentItemPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var carouselView = (CarouselView)bindable;

			var args = new CurrentItemChangedEventArgs(oldValue, newValue);

			var command = carouselView.CurrentItemChangedCommand;

			if (command != null)
			{
				var commandParameter = carouselView.CurrentItemChangedCommandParameter;

				if (command.CanExecute(commandParameter))
				{
					command.Execute(commandParameter);
				}
			}

			carouselView.CurrentItemChanged?.Invoke(carouselView, args);

			carouselView.OnCurrentItemChanged(args);
		}

		/// <summary>Bindable property for <see cref="Position"/>.</summary>
		public static readonly BindableProperty PositionProperty =
		BindableProperty.Create(nameof(Position), typeof(int), typeof(CarouselView), default(int), BindingMode.TwoWay,
			propertyChanged: PositionPropertyChanged);

		/// <summary>Bindable property for <see cref="PositionChangedCommand"/>.</summary>
		public static readonly BindableProperty PositionChangedCommandProperty =
			BindableProperty.Create(nameof(PositionChangedCommand), typeof(ICommand), typeof(CarouselView));

		/// <summary>Bindable property for <see cref="PositionChangedCommandParameter"/>.</summary>
		public static readonly BindableProperty PositionChangedCommandParameterProperty =
			BindableProperty.Create(nameof(PositionChangedCommandParameter), typeof(object),
				typeof(CarouselView));

		/// <summary>
		/// Gets or sets the index of the currently displayed item in the carousel.
		/// </summary>
		/// <value>The zero-based index of the current item. The default is 0.</value>
		/// <remarks>
		/// This property is bindable and supports two-way binding. Setting this property programmatically
		/// will scroll the carousel to display the item at the specified index.
		/// </remarks>
		public int Position
		{
			get => (int)GetValue(PositionProperty);
			set => SetValue(PositionProperty, value);
		}

		/// <summary>
		/// Gets or sets the command to execute when the carousel position changes.
		/// </summary>
		/// <value>An <see cref="ICommand"/> to execute when the position changes.</value>
		public ICommand PositionChangedCommand
		{
			get => (ICommand)GetValue(PositionChangedCommandProperty);
			set => SetValue(PositionChangedCommandProperty, value);
		}

		/// <summary>
		/// Gets or sets the parameter to pass to <see cref="PositionChangedCommand"/>.
		/// </summary>
		/// <value>The parameter object to pass to the command.</value>
		public object PositionChangedCommandParameter
		{
			get => GetValue(PositionChangedCommandParameterProperty);
			set => SetValue(PositionChangedCommandParameterProperty, value);
		}

		/// <summary>Bindable property for <see cref="ItemsLayout"/>.</summary>
		public static readonly BindableProperty ItemsLayoutProperty =
			BindableProperty.Create(nameof(ItemsLayout), typeof(LinearItemsLayout), typeof(ItemsView),
				null, defaultValueCreator: (b) => LinearItemsLayout.CreateCarouselHorizontalDefault());

		/// <summary>
		/// Gets or sets the layout used to arrange items in the carousel.
		/// </summary>
		/// <value>A <see cref="LinearItemsLayout"/> that defines the layout direction and snap behavior. The default is a horizontal layout with mandatory single snap points.</value>
		/// <remarks>
		/// The items layout determines whether items are arranged horizontally or vertically,
		/// and controls snap behavior through <see cref="ItemsLayout.SnapPointsType"/> and <see cref="ItemsLayout.SnapPointsAlignment"/>.
		/// </remarks>
		[System.ComponentModel.TypeConverter(typeof(CarouselLayoutTypeConverter))]
		public LinearItemsLayout ItemsLayout
		{
			get => (LinearItemsLayout)GetValue(ItemsLayoutProperty);
			set => SetValue(ItemsLayoutProperty, value);
		}

		/// <summary>
		/// Sets the <see cref="IndicatorView"/> to synchronize with this carousel's position and items.
		/// </summary>
		/// <value>The <see cref="IndicatorView"/> to link to this carousel.</value>
		/// <remarks>
		/// Setting this property automatically binds the indicator view's <see cref="IndicatorView.Position"/> and
		/// <see cref="IndicatorView.ItemsSource"/> to this carousel's corresponding properties.
		/// </remarks>
		[System.ComponentModel.TypeConverter(typeof(ReferenceTypeConverter))]
		public IndicatorView IndicatorView
		{
			set => LinkToIndicatorView(this, value);
		}

		static void LinkToIndicatorView(CarouselView carouselView, IndicatorView indicatorView)
		{
			if (indicatorView == null)
				return;

			indicatorView.SetBinding(IndicatorView.PositionProperty, static (CarouselView carousel) => carousel.Position, source: carouselView);
			indicatorView.SetBinding(IndicatorView.ItemsSourceProperty, static (CarouselView carousel) => carousel.ItemsSource, source: carouselView);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the carousel is currently scrolling.
		/// </summary>
		/// <value><see langword="true"/> if scrolling is in progress; otherwise, <see langword="false"/>.</value>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsScrolling { get; set; }

		/// <summary>
		/// Occurs when the <see cref="CurrentItem"/> changes.
		/// </summary>
		public event EventHandler<CurrentItemChangedEventArgs> CurrentItemChanged;

		/// <summary>
		/// Occurs when the <see cref="Position"/> changes.
		/// </summary>
		public event EventHandler<PositionChangedEventArgs> PositionChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="CarouselView"/> class.
		/// </summary>
		public CarouselView()
		{
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};
		}

		/// <summary>
		/// Called when the position changes. Override this method to add custom logic when the carousel position changes.
		/// </summary>
		/// <param name="args">Event arguments containing the previous and new position values.</param>
		protected virtual void OnPositionChanged(PositionChangedEventArgs args)
		{
		}

		/// <summary>
		/// Called when the current item changes. Override this method to add custom logic when the current item changes.
		/// </summary>
		/// <param name="args">Event arguments containing the previous and new current items.</param>
		protected virtual void OnCurrentItemChanged(EventArgs args)
		{
		}

		static void PositionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var carousel = (CarouselView)bindable;

			var args = new PositionChangedEventArgs((int)oldValue, (int)newValue);

			var command = carousel.PositionChangedCommand;

			if (command != null)
			{
				var commandParameter = carousel.PositionChangedCommandParameter;

				if (command.CanExecute(commandParameter))
				{
					command.Execute(commandParameter);
				}
			}

			carousel.PositionChanged?.Invoke(carousel, args);

			carousel.OnPositionChanged(args);
		}

		/// <summary>
		/// Sets the dragging state of the carousel. For internal use by platform renderers.
		/// </summary>
		/// <param name="value">The value to set.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsDragging(bool value)
		{
			SetValue(IsDraggingPropertyKey, value);
		}

		/// <summary>
		/// Gets a value indicating whether position changes should be animated. For internal use by platform renderers.
		/// </summary>
		/// <value><see langword="true"/> if position changes should be animated; otherwise, <see langword="false"/>.</value>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual bool AnimatePositionChanges => IsScrollAnimated;

		/// <summary>
		/// Gets a value indicating whether current item changes should be animated. For internal use by platform renderers.
		/// </summary>
		/// <value><see langword="true"/> if current item changes should be animated; otherwise, <see langword="false"/>.</value>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual bool AnimateCurrentItemChanges => IsScrollAnimated;

	}
}
