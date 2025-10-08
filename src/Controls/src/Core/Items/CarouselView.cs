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

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.ItemsView"/> whose scrollable child views 'snap' into place.</summary>
	public class CarouselView : ItemsView
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='CurrentItemVisualState']/Docs/*" />
		public const string CurrentItemVisualState = "CurrentItem";
		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='NextItemVisualState']/Docs/*" />
		public const string NextItemVisualState = "NextItem";
		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='PreviousItemVisualState']/Docs/*" />
		public const string PreviousItemVisualState = "PreviousItem";
		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='DefaultItemVisualState']/Docs/*" />
		public const string DefaultItemVisualState = "DefaultItem";

		/// <summary>Bindable property for <see cref="Loop"/>.</summary>
		public static readonly BindableProperty LoopProperty = BindableProperty.Create(nameof(Loop), typeof(bool), typeof(CarouselView), true, BindingMode.OneTime);

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='Loop']/Docs/*" />
		public bool Loop
		{
			get { return (bool)GetValue(LoopProperty); }
			set { SetValue(LoopProperty, value); }
		}

		/// <summary>Bindable property for <see cref="PeekAreaInsets"/>.</summary>
		public static readonly BindableProperty PeekAreaInsetsProperty = BindableProperty.Create(nameof(PeekAreaInsets), typeof(Thickness), typeof(CarouselView), default(Thickness));

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='PeekAreaInsets']/Docs/*" />
		public Thickness PeekAreaInsets
		{
			get { return (Thickness)GetValue(PeekAreaInsetsProperty); }
			set { SetValue(PeekAreaInsetsProperty, value); }
		}

		static readonly BindablePropertyKey VisibleViewsPropertyKey = BindableProperty.CreateReadOnly(nameof(VisibleViews), typeof(ObservableCollection<View>), typeof(CarouselView), null, defaultValueCreator: (b) => new ObservableCollection<View>());

		/// <summary>Bindable property for <see cref="VisibleViews"/>.</summary>
		public static readonly BindableProperty VisibleViewsProperty = VisibleViewsPropertyKey.BindableProperty;

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='VisibleViews']/Docs/*" />
		public ObservableCollection<View> VisibleViews => (ObservableCollection<View>)GetValue(VisibleViewsProperty);

		static readonly BindablePropertyKey IsDraggingPropertyKey = BindableProperty.CreateReadOnly(nameof(IsDragging), typeof(bool), typeof(CarouselView), false);

		/// <summary>Bindable property for <see cref="IsDragging"/>.</summary>
		public static readonly BindableProperty IsDraggingProperty = IsDraggingPropertyKey.BindableProperty;

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='IsDragging']/Docs/*" />
		public bool IsDragging => (bool)GetValue(IsDraggingProperty);

		/// <summary>Bindable property for <see cref="IsBounceEnabled"/>.</summary>
		public static readonly BindableProperty IsBounceEnabledProperty =
			BindableProperty.Create(nameof(IsBounceEnabled), typeof(bool), typeof(CarouselView), true);

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='IsBounceEnabled']/Docs/*" />
		public bool IsBounceEnabled
		{
			get { return (bool)GetValue(IsBounceEnabledProperty); }
			set { SetValue(IsBounceEnabledProperty, value); }
		}

		/// <summary>Bindable property for <see cref="IsSwipeEnabled"/>.</summary>
		public static readonly BindableProperty IsSwipeEnabledProperty =
			BindableProperty.Create(nameof(IsSwipeEnabled), typeof(bool), typeof(CarouselView), true);

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='IsSwipeEnabled']/Docs/*" />
		public bool IsSwipeEnabled
		{
			get { return (bool)GetValue(IsSwipeEnabledProperty); }
			set { SetValue(IsSwipeEnabledProperty, value); }
		}

		/// <summary>Bindable property for <see cref="IsScrollAnimated"/>.</summary>
		public static readonly BindableProperty IsScrollAnimatedProperty =
		BindableProperty.Create(nameof(IsScrollAnimated), typeof(bool), typeof(CarouselView), true);

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='IsScrollAnimated']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='CurrentItem']/Docs/*" />
		public object CurrentItem
		{
			get => GetValue(CurrentItemProperty);
			set => SetValue(CurrentItemProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='CurrentItemChangedCommand']/Docs/*" />
		public ICommand CurrentItemChangedCommand
		{
			get => (ICommand)GetValue(CurrentItemChangedCommandProperty);
			set => SetValue(CurrentItemChangedCommandProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='CurrentItemChangedCommandParameter']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='Position']/Docs/*" />
		public int Position
		{
			get => (int)GetValue(PositionProperty);
			set => SetValue(PositionProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='PositionChangedCommand']/Docs/*" />
		public ICommand PositionChangedCommand
		{
			get => (ICommand)GetValue(PositionChangedCommandProperty);
			set => SetValue(PositionChangedCommandProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='PositionChangedCommandParameter']/Docs/*" />
		public object PositionChangedCommandParameter
		{
			get => GetValue(PositionChangedCommandParameterProperty);
			set => SetValue(PositionChangedCommandParameterProperty, value);
		}

		/// <summary>Bindable property for <see cref="ItemsLayout"/>.</summary>
		public static readonly BindableProperty ItemsLayoutProperty =
			BindableProperty.Create(nameof(ItemsLayout), typeof(LinearItemsLayout), typeof(ItemsView),
				null, defaultValueCreator: (b) => LinearItemsLayout.CreateCarouselHorizontalDefault());

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='ItemsLayout']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(CarouselLayoutTypeConverter))]
		public LinearItemsLayout ItemsLayout
		{
			get => (LinearItemsLayout)GetValue(ItemsLayoutProperty);
			set => SetValue(ItemsLayoutProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='IndicatorView']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='IsScrolling']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsScrolling { get; set; }

		public event EventHandler<CurrentItemChangedEventArgs> CurrentItemChanged;
		public event EventHandler<PositionChangedEventArgs> PositionChanged;

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public CarouselView()
		{
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};
		}

		protected virtual void OnPositionChanged(PositionChangedEventArgs args)
		{
		}

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

		/// <param name="value">The value to set.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsDragging(bool value)
		{
			SetValue(IsDraggingPropertyKey, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='AnimatePositionChanges']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual bool AnimatePositionChanges => IsScrollAnimated;

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselView.xml" path="//Member[@MemberName='AnimateCurrentItemChanges']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual bool AnimateCurrentItemChanges => IsScrollAnimated;

	}
}
