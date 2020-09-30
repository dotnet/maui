using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Maps
{
	public class Map : View, IEnumerable<Pin>
	{
		public static readonly BindableProperty MapTypeProperty = BindableProperty.Create("MapType", typeof(MapType), typeof(Map), default(MapType));

		public static readonly BindableProperty IsShowingUserProperty = BindableProperty.Create("IsShowingUser", typeof(bool), typeof(Map), default(bool));

		public static readonly BindableProperty TrafficEnabledProperty = BindableProperty.Create("TrafficEnabled", typeof(bool), typeof(Map), default(bool));

		public static readonly BindableProperty HasScrollEnabledProperty = BindableProperty.Create("HasScrollEnabled", typeof(bool), typeof(Map), true);

		public static readonly BindableProperty HasZoomEnabledProperty = BindableProperty.Create("HasZoomEnabled", typeof(bool), typeof(Map), true);

		public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(Map), default(IEnumerable),
			propertyChanged: (b, o, n) => ((Map)b).OnItemsSourcePropertyChanged((IEnumerable)o, (IEnumerable)n));

		public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(Map), default(DataTemplate),
			propertyChanged: (b, o, n) => ((Map)b).OnItemTemplatePropertyChanged((DataTemplate)o, (DataTemplate)n));

		public static readonly BindableProperty ItemTemplateSelectorProperty = BindableProperty.Create(nameof(ItemTemplateSelector), typeof(DataTemplateSelector), typeof(Map), default(DataTemplateSelector),
			propertyChanged: (b, o, n) => ((Map)b).OnItemTemplateSelectorPropertyChanged());

		public static readonly BindableProperty MoveToLastRegionOnLayoutChangeProperty = BindableProperty.Create(nameof(MoveToLastRegionOnLayoutChange), typeof(bool), typeof(Map), defaultValue: true);

		readonly ObservableCollection<Pin> _pins = new ObservableCollection<Pin>();
		readonly ObservableCollection<MapElement> _mapElements = new ObservableCollection<MapElement>();
		MapSpan _visibleRegion;

		public Map(MapSpan region)
		{
			LastMoveToRegion = region;

			VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;

			_pins.CollectionChanged += PinsOnCollectionChanged;
		}

		// center on Rome by default
		public Map() : this(new MapSpan(new Position(41.890202, 12.492049), 0.1, 0.1))
		{
		}

		public bool HasScrollEnabled
		{
			get { return (bool)GetValue(HasScrollEnabledProperty); }
			set { SetValue(HasScrollEnabledProperty, value); }
		}

		public bool HasZoomEnabled
		{
			get { return (bool)GetValue(HasZoomEnabledProperty); }
			set { SetValue(HasZoomEnabledProperty, value); }
		}

		public bool IsShowingUser
		{
			get { return (bool)GetValue(IsShowingUserProperty); }
			set { SetValue(IsShowingUserProperty, value); }
		}

		public bool TrafficEnabled
		{
			get => (bool)GetValue(TrafficEnabledProperty);
			set => SetValue(TrafficEnabledProperty, value);
		}

		public MapType MapType
		{
			get { return (MapType)GetValue(MapTypeProperty); }
			set { SetValue(MapTypeProperty, value); }
		}

		public IList<Pin> Pins
		{
			get { return _pins; }
		}

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		public DataTemplateSelector ItemTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
			set { SetValue(ItemTemplateSelectorProperty, value); }
		}

		public bool MoveToLastRegionOnLayoutChange
		{
			get { return (bool)GetValue(MoveToLastRegionOnLayoutChangeProperty); }
			set { SetValue(MoveToLastRegionOnLayoutChangeProperty, value); }
		}

		public IList<MapElement> MapElements => _mapElements;

		public event EventHandler<MapClickedEventArgs> MapClicked;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendMapClicked(Position position) => MapClicked?.Invoke(this, new MapClickedEventArgs(position));

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetVisibleRegion(MapSpan value) => VisibleRegion = value;
		public MapSpan VisibleRegion
		{
			get { return _visibleRegion; }
			internal set
			{
				if (_visibleRegion == value)
					return;
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				OnPropertyChanging();
				_visibleRegion = value;
				OnPropertyChanged();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public MapSpan LastMoveToRegion { get; private set; }

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<Pin> GetEnumerator()
		{
			return _pins.GetEnumerator();
		}

		public void MoveToRegion(MapSpan mapSpan)
		{
			if (mapSpan == null)
				throw new ArgumentNullException(nameof(mapSpan));
			LastMoveToRegion = mapSpan;
			MessagingCenter.Send(this, "MapMoveToRegion", mapSpan);
		}

		void PinsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null && e.NewItems.Cast<Pin>().Any(pin => pin.Label == null))
				throw new ArgumentException("Pin must have a Label to be added to a map");
		}

		void OnItemsSourcePropertyChanged(IEnumerable oldItemsSource, IEnumerable newItemsSource)
		{
			if (oldItemsSource is INotifyCollectionChanged ncc)
			{
				ncc.CollectionChanged -= OnItemsSourceCollectionChanged;
			}

			if (newItemsSource is INotifyCollectionChanged ncc1)
			{
				ncc1.CollectionChanged += OnItemsSourceCollectionChanged;
			}

			_pins.Clear();
			CreatePinItems();
		}

		void OnItemTemplatePropertyChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
		{
			if (newItemTemplate is DataTemplateSelector)
			{
				throw new NotSupportedException(
					$"The {nameof(Map)}.{ItemTemplateProperty.PropertyName} property only supports {nameof(DataTemplate)}." +
					$" Set the {nameof(Map)}.{ItemTemplateSelectorProperty.PropertyName} property instead to use a {nameof(DataTemplateSelector)}");
			}

			_pins.Clear();
			CreatePinItems();
		}

		void OnItemTemplateSelectorPropertyChanged()
		{
			_pins.Clear();
			CreatePinItems();
		}

		void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			e.Apply(
				insert: (item, _, __) => CreatePin(item),
				removeAt: (item, _) => RemovePin(item),
				reset: () => _pins.Clear());
		}

		void CreatePinItems()
		{
			if (ItemsSource == null || (ItemTemplate == null && ItemTemplateSelector == null))
			{
				return;
			}

			foreach (object item in ItemsSource)
			{
				CreatePin(item);
			}
		}

		void CreatePin(object newItem)
		{
			DataTemplate itemTemplate = ItemTemplate;
			if (itemTemplate == null)
				itemTemplate = ItemTemplateSelector?.SelectTemplate(newItem, this);

			if (itemTemplate == null)
				return;

			var pin = (Pin)itemTemplate.CreateContent();
			pin.BindingContext = newItem;
			_pins.Add(pin);
		}

		void RemovePin(object itemToRemove)
		{
			// Instead of just removing by item (i.e. _pins.Remove(pinToRemove))
			//  we need to remove by index because of how Pin.Equals() works
			for (int i = 0; i < _pins.Count; ++i)
			{
				Pin pin = _pins[i];
				if (pin.BindingContext?.Equals(itemToRemove) == true)
				{
					_pins.RemoveAt(i);
				}
			}
		}
	}
}