using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.Maps
{
	/// <summary>
	/// The Map control is a cross-platform view for displaying and annotating maps.
	/// </summary>
	public partial class Map : View
	{
		/// <summary>Bindable property for <see cref="MapType"/>.</summary>
		public static readonly BindableProperty MapTypeProperty = BindableProperty.Create(nameof(MapType), typeof(MapType), typeof(Map), default(MapType));

		/// <summary>Bindable property for <see cref="IsShowingUser"/>.</summary>
		public static readonly BindableProperty IsShowingUserProperty = BindableProperty.Create(nameof(IsShowingUser), typeof(bool), typeof(Map), default(bool));

		/// <summary>Bindable property for <see cref="IsTrafficEnabled"/>.</summary>
		public static readonly BindableProperty IsTrafficEnabledProperty = BindableProperty.Create(nameof(IsTrafficEnabled), typeof(bool), typeof(Map), default(bool));

		/// <summary>Bindable property for <see cref="IsScrollEnabled"/>.</summary>
		public static readonly BindableProperty IsScrollEnabledProperty = BindableProperty.Create(nameof(IsScrollEnabled), typeof(bool), typeof(Map), true);

		/// <summary>Bindable property for <see cref="IsZoomEnabled"/>.</summary>
		public static readonly BindableProperty IsZoomEnabledProperty = BindableProperty.Create(nameof(IsZoomEnabled), typeof(bool), typeof(Map), true);

		/// <summary>Bindable property for <see cref="ItemsSource"/>.</summary>
		public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(Map), default(IEnumerable),
			propertyChanged: (b, o, n) => ((Map)b).OnItemsSourcePropertyChanged((IEnumerable)o, (IEnumerable)n));

		/// <summary>Bindable property for <see cref="ItemTemplate"/>.</summary>
		public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(Map), default(DataTemplate),
			propertyChanged: (b, o, n) => ((Map)b).OnItemTemplatePropertyChanged((DataTemplate)o, (DataTemplate)n));

		/// <summary>Bindable property for <see cref="ItemTemplateSelector"/>.</summary>
		public static readonly BindableProperty ItemTemplateSelectorProperty = BindableProperty.Create(nameof(ItemTemplateSelector), typeof(DataTemplateSelector), typeof(Map), default(DataTemplateSelector),
			propertyChanged: (b, o, n) => ((Map)b).OnItemTemplateSelectorPropertyChanged());

		readonly ObservableCollection<Pin> _pins = new();
		readonly ObservableCollection<MapElement> _mapElements = new();
		MapSpan? _visibleRegion;
		MapSpan? _lastMoveToRegion;

		/// <summary>
		/// Initializes a new instance of the <see cref="Map"/> class with a region.
		/// </summary>
		/// <param name="region">The region that should be initially shown by the map.</param>
		public Map(MapSpan region)
		{
			MoveToRegion(region);
#pragma warning disable CS0618 // Type or member is obsolete
			VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;
#pragma warning restore CS0618 // Type or member is obsolete

			_pins.CollectionChanged += PinsOnCollectionChanged;
			_mapElements.CollectionChanged += MapElementsCollectionChanged;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Map"/> class with a region.
		/// </summary>
		// <remarks>The selected region will default to Maui, Hawaii.</remarks>
		public Map() : this(new MapSpan(new Devices.Sensors.Location(20.793062527, -156.336394697), 0.5, 0.5))
		{
		}

		/// <summary>
		/// Gets or sets a value that indicates if scrolling by user input is enabled. Default value is <see langword="true"/>.
		/// This is a bindable property.
		/// </summary>
		public bool IsScrollEnabled
		{
			get { return (bool)GetValue(IsScrollEnabledProperty); }
			set { SetValue(IsScrollEnabledProperty, value); }
		}

		/// <summary>
		/// Gets or sets a value that indicates if zooming by user input is enabled. Default value is <see langword="true"/>.
		/// This is a bindable property.
		/// </summary>
		public bool IsZoomEnabled
		{
			get { return (bool)GetValue(IsZoomEnabledProperty); }
			set { SetValue(IsZoomEnabledProperty, value); }
		}

		/// <summary>
		/// Gets or sets a value that indicates if the map shows an indicator of the current position of this device. Default value is <see langword="false"/>
		/// This is a bindable property.
		/// </summary>
		/// <remarks>Depending on the platform it is likely that runtime permission(s) need to be requested to determine the current location of the device.</remarks>
		public bool IsShowingUser
		{
			get { return (bool)GetValue(IsShowingUserProperty); }
			set { SetValue(IsShowingUserProperty, value); }
		}

		/// <summary>
		/// Gets or sets a value that indicates if the map shows current traffic information. Default value is <see langword="false"/>.
		/// This is a bindable property.
		/// </summary>
		public bool IsTrafficEnabled
		{
			get => (bool)GetValue(IsTrafficEnabledProperty);
			set => SetValue(IsTrafficEnabledProperty, value);
		}

		/// <summary>
		/// Gets or sets the style of the map. Default value is <see cref="MapType.Street"/>. 
		/// This is a bindable property.
		/// </summary>
		public MapType MapType
		{
			get { return (MapType)GetValue(MapTypeProperty); }
			set { SetValue(MapTypeProperty, value); }
		}

		/// <summary>
		/// Gets the pins currently added to this map.
		/// </summary>
		public IList<Pin> Pins
		{
			get { return _pins; }
		}

		/// <summary>
		/// Gets or sets the object that represents the collection of pins that should be shown on the map.
		/// This is a bindable property.
		/// </summary>
		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		/// <summary>
		/// Gets or sets the template that is to be applied to each object in <see cref="ItemsSource"/>.
		/// This is a bindable property.
		/// </summary>
		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		/// <summary>
		/// Gets or sets the object that selects the template that is to be applied to each object in <see cref="ItemsSource"/>.
		/// This is a bindable property.
		/// </summary>
		public DataTemplateSelector ItemTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
			set { SetValue(ItemTemplateSelectorProperty, value); }
		}

		/// <summary>
		/// Gets the elements (pins, polygons, polylines, etc.) currently added to this map.
		/// </summary>
		public IList<MapElement> MapElements => _mapElements;

		/// <summary>
		/// Occurs when the user clicks/taps on the map control.
		/// </summary>
		public event EventHandler<MapClickedEventArgs>? MapClicked;

		/// <summary>
		/// Gets the currently visible region of the map.
		/// </summary>
		public MapSpan? VisibleRegion
		{
			get { return _visibleRegion; }
		}

		/// <summary>
		/// Returns an enumarator of all the pins that are currently added to the map.
		/// </summary>
		/// <returns>An instance of <see cref="IEnumerator{IMapPin}"/>.</returns>
		public IEnumerator<IMapPin> GetEnumerator()
		{
			return _pins.GetEnumerator();
		}

		/// <summary>
		/// Adjusts the viewport of the map control to view the specified region.
		/// </summary>
		/// <param name="mapSpan">A <see cref="MapSpan"/> object containing details on what region should be shown.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="mapSpan"/> is <see langword="null"/>.</exception>
		public void MoveToRegion(MapSpan mapSpan)
		{
			if (mapSpan is null)
			{
				throw new ArgumentNullException(nameof(mapSpan));
			}

			_lastMoveToRegion = mapSpan;
			Handler?.Invoke(nameof(IMap.MoveToRegion), _lastMoveToRegion);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		void SetVisibleRegion(MapSpan? visibleRegion)
		{
			if (visibleRegion is null)
			{
				throw new ArgumentNullException(nameof(visibleRegion));
			}

			if (_visibleRegion == visibleRegion)
			{
				return;
			}

			OnPropertyChanging(nameof(VisibleRegion));
			_visibleRegion = visibleRegion;
			OnPropertyChanged(nameof(VisibleRegion));
		}

		void PinsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems is not null && e.NewItems.Cast<Pin>().Any(pin => pin.Label is null))
			{
				throw new ArgumentException("Pin must have a Label to be added to a map");
			}

			Handler?.UpdateValue(nameof(IMap.Pins));
		}

		void MapElementsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			Handler?.UpdateValue(nameof(IMap.Elements));
			if (e.NewItems is not null)
			{
				foreach (MapElement item in e.NewItems)
				{
					item.PropertyChanged += MapElementPropertyChanged;
				}
			}

			if (e.OldItems is not null)
			{
				foreach (MapElement item in e.OldItems)
				{
					item.PropertyChanged -= MapElementPropertyChanged;
				}
			}
		}

		void MapElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (sender is MapElement mapElement)
			{
				var index = MapElements.IndexOf(mapElement);
				var args = new Maui.Maps.Handlers.MapElementHandlerUpdate(index, mapElement);
				Handler?.Invoke(nameof(Maui.Maps.Handlers.IMapHandler.UpdateMapElement), args);
			}
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

		void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			e.Apply(
				insert: (item, _, __) => CreatePin(item),
				removeAt: (item, _) => RemovePin(item),
				reset: () => _pins.Clear());
			Handler?.UpdateValue(nameof(IMap.Pins));
		}

		void CreatePinItems()
		{
			if (ItemsSource is null || (ItemTemplate is null && ItemTemplateSelector is null))
			{
				return;
			}

			foreach (object item in ItemsSource)
			{
				CreatePin(item);
			}

			Handler?.UpdateValue(nameof(IMap.Pins));
		}

		void CreatePin(object newItem)
		{
			DataTemplate? itemTemplate = ItemTemplate;
			if (itemTemplate is null)
			{
				itemTemplate = ItemTemplateSelector?.SelectTemplate(newItem, this);
			}

			if (itemTemplate is null)
			{
				return;
			}

			var pin = (Pin)itemTemplate.CreateContent();
			pin.BindingContext = newItem;
			_pins.Add(pin);
		}

		void RemovePin(object itemToRemove)
		{
			//// Instead of just removing by item (i.e. _pins.Remove(pinToRemove))
			////  we need to remove by index because of how Pin.Equals() works
			for (int i = 0; i < _pins.Count; ++i)
			{
				Pin? pin = _pins[i] as Pin;
				if (pin is not null)
				{
					if (pin.BindingContext?.Equals(itemToRemove) == true)
					{
						_pins.RemoveAt(i);
					}
				}
			}
		}
	}
}