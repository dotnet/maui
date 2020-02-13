using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms.Platform;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_CarouselViewRenderer))]
	public class CarouselView : ItemsView
	{
		public const string CurrentItemVisualState = "CurrentItem";
		public const string NextItemVisualState = "NextItem";
		public const string PreviousItemVisualState = "PreviousItem";
		public const string DefaultItemVisualState = "DefaultItem";

		bool _isInitialized;
		int _gotoPosition = -1;

		public static readonly BindableProperty PeekAreaInsetsProperty = BindableProperty.Create(nameof(PeekAreaInsets), typeof(Thickness), typeof(CarouselView), default(Thickness));

		public Thickness PeekAreaInsets
		{
			get { return (Thickness)GetValue(PeekAreaInsetsProperty); }
			set { SetValue(PeekAreaInsetsProperty, value); }
		}

		static readonly BindablePropertyKey VisibleViewsPropertyKey = BindableProperty.CreateReadOnly(nameof(VisibleViews), typeof(ObservableCollection<View>), typeof(CarouselView), new ObservableCollection<View>());

		public static readonly BindableProperty VisibleViewsProperty = VisibleViewsPropertyKey.BindableProperty;

		public ObservableCollection<View> VisibleViews => (ObservableCollection<View>)GetValue(VisibleViewsProperty);

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

			var positionItem = GetPositionForItem(carouselView, newValue);
			var gotoPosition = carouselView._gotoPosition;

			if (positionItem == gotoPosition || gotoPosition == -1)
			{
				carouselView._gotoPosition = -1;
				carouselView.SetValueCore(PositionProperty, positionItem);
			}

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

		public static readonly BindableProperty ItemsLayoutProperty =
			BindableProperty.Create(nameof(ItemsLayout), typeof(LinearItemsLayout), typeof(ItemsView),
				LinearItemsLayout.CarouselDefault);

		[TypeConverter(typeof(CarouselLayoutTypeConverter))]
		public LinearItemsLayout ItemsLayout
		{
			get => (LinearItemsLayout)GetValue(ItemsLayoutProperty);
			set => SetValue(ItemsLayoutProperty, value);
		}

		[TypeConverter(typeof(ReferenceTypeConverter))]
		public IndicatorView IndicatorView
		{
			set => LinkToIndicatorView(this, value);
		}

		static void LinkToIndicatorView(CarouselView carouselView, IndicatorView indicatorView)
		{
			if (indicatorView == null)
				return;

			indicatorView.SetBinding(IndicatorView.PositionProperty, new Binding
			{
				Path = nameof(CarouselView.Position),
				Source = carouselView
			});

			indicatorView.SetBinding(IndicatorView.ItemsSourceProperty, new Binding
			{
				Path = nameof(ItemsView.ItemsSource),
				Source = carouselView
			});
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsScrolling { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public Queue<Action> ScrollToActions = new Queue<Action>();

		public event EventHandler<CurrentItemChangedEventArgs> CurrentItemChanged;
		public event EventHandler<PositionChangedEventArgs> PositionChanged;

		public CarouselView()
		{
			VerifyCarouselViewFlagEnabled(constructorHint: nameof(CarouselView));
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void VerifyCarouselViewFlagEnabled(
			string constructorHint = null,
			[CallerMemberName] string memberName = "")
		{
			try
			{
				ExperimentalFlags.VerifyFlagEnabled(nameof(CollectionView), ExperimentalFlags.CarouselViewExperimental,
					constructorHint, memberName);
			}
			catch (InvalidOperationException)
			{

			}
		}

		protected virtual void OnPositionChanged(PositionChangedEventArgs args)
		{
		}

		protected virtual void OnCurrentItemChanged(EventArgs args)
		{
		}

		protected override void OnScrolled(ItemsViewScrolledEventArgs e)
		{
			SetCurrentItem(GetItemForPosition(this, e.CenterItemIndex));

			base.OnScrolled(e);
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

			if (args.CurrentPosition == carousel._gotoPosition)
				carousel._gotoPosition = -1;

			// User is interacting with the carousel we don't need to scroll to item 
			if (!carousel.IsDragging && !carousel.IsScrolling)
			{
				carousel._gotoPosition = args.CurrentPosition;

				Action actionSCroll = () =>
				{
					carousel.ScrollTo(args.CurrentPosition, position: ScrollToPosition.Center, animate: carousel.IsScrollAnimated);
				};

				if (!carousel._isInitialized)
					carousel.ScrollToActions.Enqueue(actionSCroll);
				else
					actionSCroll();
			}

			carousel.OnPositionChanged(args);
		}


		static object GetItemForPosition(CarouselView carouselView, int index)
		{
			if (!(carouselView?.ItemsSource is IList itemSource))
				return null;

			if (itemSource.Count == 0)
				return null;

			return itemSource[index];
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetCurrentItem(object item)
		{
			SetValueFromRenderer(CurrentItemProperty, item);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsDragging(bool value)
		{
			SetValue(IsDraggingPropertyKey, value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void PlatformInitialized()
		{
			_isInitialized = true;
		}
	}
}
