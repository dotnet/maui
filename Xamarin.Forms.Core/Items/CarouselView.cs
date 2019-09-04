using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_CarouselViewRenderer))]
	public class CarouselView : ItemsView
	{
		public const string CurrentItemVisualState = "CurrentItem";
		public const string NextItemVisualState = "NextItem";
		public const string PreviousItemVisualState = "PreviousItem";
		public const string VisibleItemVisualState = "VisibleItem";
		public const string DefaultItemVisualState = "DefaultItem";

		public static readonly BindableProperty PeekAreaInsetsProperty = BindableProperty.Create(nameof(PeekAreaInsets), typeof(Thickness), typeof(CarouselView), default(Thickness));

		public Thickness PeekAreaInsets
		{
			get { return (Thickness)GetValue(PeekAreaInsetsProperty); }
			set { SetValue(PeekAreaInsetsProperty, value); }
		}

		static readonly BindablePropertyKey VisibleViewsPropertyKey = BindableProperty.CreateReadOnly(nameof(VisibleViews), typeof(List<View>), typeof(CarouselView), null);

		public static readonly BindableProperty VisibleViewsProperty = VisibleViewsPropertyKey.BindableProperty;

		public List<View> VisibleViews => (List<View>)GetValue(VisibleViewsProperty);

		static readonly BindablePropertyKey IsDraggingPropertyKey = BindableProperty.CreateReadOnly(nameof(IsDragging), typeof(bool), typeof(CarouselView), false);

		public static readonly BindableProperty IsDraggingProperty = IsDraggingPropertyKey.BindableProperty;

		public bool IsDragging => (bool)GetValue(IsDraggingProperty);

		public static readonly BindableProperty IsBounceEnabledProperty =
			BindableProperty.Create(nameof(IsBounceEnabled), typeof(bool), typeof(CarouselView), true);

		public bool IsBounceEnabled
		{
			get { return (bool)GetValue(IsBounceEnabledProperty); }
			set { SetValue(IsBounceEnabledProperty, value); }
		}

		public static readonly BindableProperty NumberOfSideItemsProperty =
			BindableProperty.Create(nameof(NumberOfSideItems), typeof(int), typeof(CarouselView), 0);

		public int NumberOfSideItems
		{
			get { return (int)GetValue(NumberOfSideItemsProperty); }
			set { SetValue(NumberOfSideItemsProperty, value); }
		}

		public static readonly BindableProperty IsSwipeEnabledProperty =
			BindableProperty.Create(nameof(IsSwipeEnabled), typeof(bool), typeof(CarouselView), true);

		public bool IsSwipeEnabled
		{
			get { return (bool)GetValue(IsSwipeEnabledProperty); }
			set { SetValue(IsSwipeEnabledProperty, value); }
		}

		public static readonly BindableProperty IsScrollAnimatedProperty =
		BindableProperty.Create(nameof(IsScrollAnimated), typeof(bool), typeof(CarouselView), true);

		public bool IsScrollAnimated
		{
			get { return (bool)GetValue(IsScrollAnimatedProperty); }
			set { SetValue(IsScrollAnimatedProperty, value); }
		}

		public static readonly BindableProperty CurrentItemProperty =
		BindableProperty.Create(nameof(CurrentItem), typeof(object), typeof(CarouselView), default, BindingMode.TwoWay, 
			propertyChanged: CurrentItemPropertyChanged);

		public static readonly BindableProperty CurrentItemChangedCommandProperty =
			BindableProperty.Create(nameof(CurrentItemChangedCommand), typeof(ICommand), typeof(CarouselView));

		public static readonly BindableProperty CurrentItemChangedCommandParameterProperty =
			BindableProperty.Create(nameof(CurrentItemChangedCommandParameter), typeof(object), typeof(CarouselView));

		public object CurrentItem
		{
			get => GetValue(CurrentItemProperty);
			set => SetValue(CurrentItemProperty, value);
		}

		public ICommand CurrentItemChangedCommand
		{
			get => (ICommand)GetValue(CurrentItemChangedCommandProperty);
			set => SetValue(CurrentItemChangedCommandProperty, value);
		}

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

			carouselView.SetValueCore(PositionProperty, GetPositionForItem(carouselView, newValue));

			carouselView.CurrentItemChanged?.Invoke(carouselView, args);

			carouselView.OnCurrentItemChanged(args);
		}

		public static readonly BindableProperty PositionProperty =
		BindableProperty.Create(nameof(Position), typeof(int), typeof(CarouselView), default(int), BindingMode.TwoWay,
			propertyChanged: PositionPropertyChanged);

		public static readonly BindableProperty PositionChangedCommandProperty =
			BindableProperty.Create(nameof(PositionChangedCommand), typeof(ICommand), typeof(CarouselView));

		public static readonly BindableProperty PositionChangedCommandParameterProperty =
			BindableProperty.Create(nameof(PositionChangedCommandParameter), typeof(object),
				typeof(CarouselView));

		public int Position
		{
			get => (int)GetValue(PositionProperty);
			set => SetValue(PositionProperty, value);
		}

		public ICommand PositionChangedCommand
		{
			get => (ICommand)GetValue(PositionChangedCommandProperty);
			set => SetValue(PositionChangedCommandProperty, value);
		}

		public object PositionChangedCommandParameter
		{
			get => GetValue(PositionChangedCommandParameterProperty);
			set => SetValue(PositionChangedCommandParameterProperty, value);
		}

		public event EventHandler<CurrentItemChangedEventArgs> CurrentItemChanged;
		public event EventHandler<PositionChangedEventArgs> PositionChanged;

		public CarouselView()
		{
			CollectionView.VerifyCollectionViewFlagEnabled(constructorHint: nameof(CarouselView));
			ItemsLayout = new ListItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};
			ItemSizingStrategy = ItemSizingStrategy.None;
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

			//user is interacting with the carousel we don't need to scroll to item 
			if (!carousel.IsDragging)
				carousel.ScrollTo(args.CurrentPosition, position: ScrollToPosition.Center, animate: carousel.IsScrollAnimated);

			carousel.OnPositionChanged(args);
		}

		static int GetPositionForItem(CarouselView carouselView, object item)
		{
			var itemSource = carouselView?.ItemsSource as IList;

			for (int n = 0; n < itemSource?.Count; n++)
			{
				if (itemSource[n] == item)
				{
					return n;
				}
			}
			return 0;
		}

		public void SetCurrentItem(object item)
		{
			SetValueFromRenderer(CurrentItemProperty, item);
		}

		public void SetIsDragging(bool value)
		{
			SetValue(IsDraggingPropertyKey, value);
		}
	}
}