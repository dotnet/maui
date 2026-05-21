using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class MapTests : BaseTestFixture
	{
		[Fact]
		public void AddPin()
		{
			var map = new Map();

			var home = new Pin
			{
				Label = "Home",
				Location = new Location(88, 2),
				Type = PinType.Place,
				Address = "123 My Place"
			};

			map.Pins.Add(home);

			Assert.Single(map.Pins);
			Assert.Equal("Home", map.Pins[0].Label);
			var mall = new Pin
			{
				Label = "Mall",
				Location = new Location(-12, -67),
				Type = PinType.Place,
				Address = "123 Fun"
			};

			map.Pins.Add(mall);

			Assert.Equal(2, map.Pins.Count);
			Assert.Equal(-12, map.Pins[1].Location.Latitude);
		}

		[Fact]
		public void AddPinWithoutName()
		{
			var map = new Map();
			var noNamePin = new Pin
			{
				Location = new Location(50, 50),
				Type = PinType.Generic,
				Address = "123 Fun"
			};

			var exception = Assert.Throws<ArgumentException>(() => map.Pins.Add(noNamePin));
			Assert.Equal("Pin must have a Label to be added to a map", exception.Message);
		}

		[Fact]
		public void AddPinWithoutAddress()
		{
			var map = new Map();
			var noAddressPin = new Pin
			{
				Location = new Location(37.9, -20.87),
				Label = "I have no address",
				Type = PinType.SearchResult
			};

			map.Pins.Add(noAddressPin);
			Assert.Single(map.Pins);
			Assert.Equal("I have no address", map.Pins[0].Label);
			Assert.Null(map.Pins[0].Address);
		}

		[Fact]
		public void RemovePin()
		{
			var map = new Map();
			var genericPlace = new Pin
			{
				Label = "Generic",
				Location = new Location(-12, -67),
				Type = PinType.Generic,
				Address = "XXX"
			};

			var mall = new Pin
			{
				Label = "Mall",
				Location = new Location(-29, -87),
				Type = PinType.Place,
				Address = "123 Fun"
			};

			map.Pins.Add(genericPlace);
			Assert.Single(map.Pins);

			map.Pins.Add(mall);
			Assert.Equal(2, map.Pins.Count);

			map.Pins.Remove(genericPlace);
			Assert.Single(map.Pins);

			Assert.True(map.Pins.Contains(mall));
			Assert.False(map.Pins.Contains(genericPlace));
		}

		[Fact]
		public void VisibleRegionDoubleSetShouldntTriggerChange()
		{
			var map = new Map();
			((IMap)map).VisibleRegion = MapSpan.FromCenterAndRadius(new Location(1, 1), Distance.FromKilometers(1));
			bool signaled = false;
			map.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "VisibleRegion")
					signaled = true;
			};

			((IMap)map).VisibleRegion = MapSpan.FromCenterAndRadius(new Location(1, 1), Distance.FromKilometers(1));

			Assert.False(signaled);
		}

		[Fact]
		public void TracksEmpty()
		{
			var map = new Map();

			var itemsSource = new ObservableCollection<int>();
			map.ItemsSource = itemsSource;
			map.ItemTemplate = new DataTemplate();

			Assert.True(IsMapWithItemsSource(itemsSource, map));
		}

		[Fact]
		public void TracksAdd()
		{
			var itemsSource = new ObservableCollection<int>();

			var map = new Map()
			{
				ItemsSource = itemsSource,
				ItemTemplate = GetItemTemplate()
			};

			itemsSource.Add(1);
			Assert.True(IsMapWithItemsSource(itemsSource, map));
		}

		[Fact]
		public void TracksInsert()
		{
			var itemsSource = new ObservableCollection<int>();

			var map = new Map()
			{
				ItemsSource = itemsSource,
				ItemTemplate = GetItemTemplate()
			};

			itemsSource.Insert(0, 1);
			Assert.True(IsMapWithItemsSource(itemsSource, map));
		}

		[Fact]
		public void TracksRemove()
		{
			var itemsSource = new ObservableCollection<int>() { 0, 1 };

			var map = new Map()
			{
				ItemsSource = itemsSource,
				ItemTemplate = GetItemTemplate()
			};

			itemsSource.RemoveAt(0);
			Assert.True(IsMapWithItemsSource(itemsSource, map));

			itemsSource.Remove(1);
			Assert.True(IsMapWithItemsSource(itemsSource, map));
		}

		[Fact]
		public void TracksReplace()
		{
			var itemsSource = new ObservableCollection<int>() { 0, 1, 2 };

			var map = new Map()
			{
				ItemsSource = itemsSource,
				ItemTemplate = GetItemTemplate()
			};

			itemsSource[0] = 3;
			itemsSource[1] = 4;
			itemsSource[2] = 5;
			Assert.True(IsMapWithItemsSource(itemsSource, map));
		}

		[Fact]
		public void ItemMove()
		{
			var itemsSource = new ObservableCollection<int>() { 0, 1 };

			var map = new Map()
			{
				ItemsSource = itemsSource,
				ItemTemplate = GetItemTemplate()
			};

			itemsSource.Move(0, 1);
			Assert.True(IsMapWithItemsSource(itemsSource, map));

			itemsSource.Move(1, 0);
			Assert.True(IsMapWithItemsSource(itemsSource, map));
		}

		[Fact]
		public void TracksClear()
		{
			var itemsSource = new ObservableCollection<int>() { 0, 1 };

			var map = new Map()
			{
				ItemsSource = itemsSource,
				ItemTemplate = GetItemTemplate()
			};

			itemsSource.Clear();
			Assert.True(IsMapWithItemsSource(itemsSource, map));
		}

		[Fact]
		public void TracksNull()
		{
			var map = new Map()
			{
				ItemTemplate = GetItemTemplate()
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			map.ItemsSource = itemsSource;
			Assert.True(IsMapWithItemsSource(itemsSource, map));

			itemsSource = null;
			map.ItemsSource = itemsSource;
			Assert.True(IsMapWithItemsSource(itemsSource, map));
		}

		[Fact]
		public void TracksItemTemplate()
		{
			var map = new Map()
			{
				ItemTemplate = GetItemTemplate()
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 3));
			map.ItemsSource = itemsSource;
			Assert.True(IsMapWithItemsSource(itemsSource, map));
			foreach (IMapPin pin in map.Pins)
			{
				Assert.True(pin.Address == "Address");
			}

			map.ItemTemplate = GetItemTemplate("Address 2");
			Assert.True(IsMapWithItemsSource(itemsSource, map));
			foreach (IMapPin pin in map.Pins)
			{
				Assert.True(pin.Address == "Address 2");
			}
		}

		[Fact]
		public void ItemTemplateSelectorIsSet()
		{
			var map = new Map();

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 3));
			map.ItemsSource = itemsSource;
			map.ItemTemplateSelector = new TestDataTemplateSelector("Address 2");

			Assert.True(IsMapWithItemsSource(itemsSource, map));
			foreach (IMapPin pin in map.Pins)
			{
				Assert.True(pin.Address == "Address 2");
			}
		}

		[Fact]
		public void ItemTemplateTakesPrecendenceOverItemTemplateSelector()
		{
			var map = new Map();

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			map.ItemsSource = itemsSource;
			map.ItemTemplate = GetItemTemplate("Address 1");
			map.ItemTemplateSelector = new TestDataTemplateSelector("Address 2");

			Assert.True(IsMapWithItemsSource(itemsSource, map));
			foreach (IMapPin pin in map.Pins)
			{
				Assert.Equal("Address 1", pin.Address);
			}
		}

		[Fact]
		public void ItemsSourceTakePrecendenceOverPins()
		{
			var map = new Map()
			{
				ItemTemplate = GetItemTemplate()
			};

			map.Pins.Add(new Pin() { Label = "Label" });
			map.Pins.Add(new Pin() { Label = "Label" });

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			map.ItemsSource = itemsSource;
			Assert.True(IsMapWithItemsSource(itemsSource, map));
		}

		[Fact, Category(TestCategory.Memory)]
		public async Task ElementIsGarbageCollectedAfterItsRemoved()
		{
			var pageRoot = new Grid();
			var page = new ContentPage() { Content = pageRoot };

			WeakReference CreateReference()
			{
				var map = new Map()
				{
					ItemTemplate = GetItemTemplate()
				};

				// Create a view-model and bind the map to it
				map.SetBinding(Map.ItemsSourceProperty, new Binding(nameof(MockViewModel.Items)));
				map.BindingContext = new MockViewModel(new ObservableCollection<int>(Enumerable.Range(0, 10)));

				// Set ItemsSource
				var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
				Assert.True(IsMapWithItemsSource(itemsSource, map));

				// Add the map to the container
				pageRoot.Children.Add(map);

				var weakReference = new WeakReference(map);

				// Remove map from container
				pageRoot.Children.Remove(map);

				return weakReference;
			}

			var weakReference = CreateReference();

			await TestHelpers.Collect();

			Assert.False(weakReference.IsAlive);

			GC.KeepAlive(page);
		}

		[Fact]
		public void ThrowsExceptionOnUsingDataTemplateSelectorForItemTemplate()
		{
			var map = new Map();

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			map.ItemsSource = itemsSource;

			Assert.Throws<NotSupportedException>(() => map.ItemTemplate = GetDataTemplateSelector());
		}

		[Fact]
		public void DontTrackAfterItemsSourceChanged()
		{
			var map = new Map()
			{
				ItemTemplate = GetItemTemplate()
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			map.ItemsSource = itemsSource;
			map.ItemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));

			itemsSource.Add(11);
			Assert.True(itemsSource.Count() == 11);
		}

		[Fact]
		public void WorksWithNullItems()
		{
			var map = new Map()
			{
				ItemTemplate = GetItemTemplate()
			};

			var itemsSource = new ObservableCollection<int?>(Enumerable.Range(0, 10).Cast<int?>());
			itemsSource.Add(null);
			map.ItemsSource = itemsSource;
			Assert.True(IsMapWithItemsSource(itemsSource, map));
		}

		[Fact]
		public void MapElementIsVisibleDefaultIsTrue()
		{
			var polygon = new Polygon();
			Assert.True(polygon.IsVisible);
		}

		[Fact]
		public void MapElementIsVisibleCanBeSet()
		{
			var polygon = new Polygon();
			polygon.IsVisible = false;
			Assert.False(polygon.IsVisible);
		}

		[Fact]
		public void MapElementZIndexDefaultIsZero()
		{
			var polyline = new Polyline();
			Assert.Equal(0, polyline.ZIndex);
		}

		[Fact]
		public void MapElementZIndexCanBeSet()
		{
			var circle = new Circle
			{
				Center = new Location(0, 0),
				Radius = new Distance(100)
			};
			circle.ZIndex = 5;
			Assert.Equal(5, circle.ZIndex);
		}

		[Fact]
		public void MapElementIsVisibleWorksOnAllTypes()
		{
			var polygon = new Polygon { IsVisible = false };
			var polyline = new Polyline { IsVisible = false };
			var circle = new Circle
			{
				Center = new Location(0, 0),
				Radius = new Distance(100),
				IsVisible = false
			};

			Assert.False(polygon.IsVisible);
			Assert.False(polyline.IsVisible);
			Assert.False(circle.IsVisible);
		}

		[Fact]
		public void MapStyleDefaultIsNull()
		{
			var map = new Map();
			Assert.Null(map.MapStyle);
		}

		[Fact]
		public void MapStyleCanBeSet()
		{
			var map = new Map();
			var style = "[{\"featureType\":\"water\",\"stylers\":[{\"color\":\"#00ff00\"}]}]";
			map.MapStyle = style;
			Assert.Equal(style, map.MapStyle);
		}

		[Fact]
		public void MapStyleCanBeCleared()
		{
			var map = new Map();
			map.MapStyle = "[{\"featureType\":\"water\",\"stylers\":[{\"color\":\"#00ff00\"}]}]";
			map.MapStyle = null;
			Assert.Null(map.MapStyle);
		}

		// Checks if for every item in the items source there's a corresponding pin
		static bool IsMapWithItemsSource(IEnumerable itemsSource, Map map)
		{
			if (itemsSource == null)
			{
				return true;
			}

			if (map.ItemTemplate == null && map.ItemTemplateSelector == null)
			{
				// If ItemsSource is set but neither ItemTemplate and ItemTemplate is, there should not be any Pins
				return !map.Pins.Any();
			}

			int i = 0;
			foreach (object item in itemsSource)
			{
				// Pins collection order is not tracked, so just make sure a Pin for item exists
				if (!map.Pins.Any(p => Equals(item, (p as Pin).BindingContext)))
				{
					return false;
				}

				++i;
			}

			return map.Pins.Count == i;
		}

		static DataTemplate GetItemTemplate(string address = null)
		{
			return new DataTemplate(() => new Pin()
			{
				Address = address ?? "Address",
				Label = "Label",
				Location = new Location()
			});
		}

		static DataTemplateSelector GetDataTemplateSelector(string address = null)
		{
			return new TestDataTemplateSelector(address);
		}

		class TestDataTemplateSelector : DataTemplateSelector
		{
			readonly DataTemplate dt;

			public TestDataTemplateSelector(string address = null)
			{
				dt = GetItemTemplate(address);
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				return dt;
			}
		}

		class MockViewModel
		{
			public IEnumerable Items { get; }
			public MockViewModel(IEnumerable itemsSource)
			{
				Items = itemsSource;
			}
		}

		[Fact]
		public void CircleClickedEventFires()
		{
			var circle = new Circle
			{
				Center = new Location(37.79752, -122.40183),
				Radius = new Distance(200),
			};

			bool eventFired = false;
			circle.CircleClicked += (s, e) => eventFired = true;

			((IMapElement)circle).Clicked();

			Assert.True(eventFired);
		}

		[Fact]
		public void PolygonClickedEventFires()
		{
			var polygon = new Polygon();
			polygon.Geopath.Add(new Location(37.7997, -122.4050));
			polygon.Geopath.Add(new Location(37.7997, -122.3980));
			polygon.Geopath.Add(new Location(37.7950, -122.4015));

			bool eventFired = false;
			polygon.PolygonClicked += (s, e) => eventFired = true;

			((IMapElement)polygon).Clicked();

			Assert.True(eventFired);
		}

		[Fact]
		public void PolylineClickedEventFires()
		{
			var polyline = new Polyline();
			polyline.Geopath.Add(new Location(37.7930, -122.4100));
			polyline.Geopath.Add(new Location(37.7940, -122.4050));

			bool eventFired = false;
			polyline.PolylineClicked += (s, e) => eventFired = true;

			((IMapElement)polyline).Clicked();

			Assert.True(eventFired);
		}

		[Fact]
		public void CircleClickedEventSenderIsCircle()
		{
			var circle = new Circle
			{
				Center = new Location(37.79752, -122.40183),
				Radius = new Distance(200),
			};

			object sender = null;
			circle.CircleClicked += (s, e) => sender = s;

			((IMapElement)circle).Clicked();

			Assert.Same(circle, sender);
		}

		[Fact]
		public void PolygonClickedEventSenderIsPolygon()
		{
			var polygon = new Polygon();

			object sender = null;
			polygon.PolygonClicked += (s, e) => sender = s;

			((IMapElement)polygon).Clicked();

			Assert.Same(polygon, sender);
		}

		[Fact]
		public void PolylineClickedEventSenderIsPolyline()
		{
			var polyline = new Polyline();

			object sender = null;
			polyline.PolylineClicked += (s, e) => sender = s;

			((IMapElement)polyline).Clicked();

			Assert.Same(polyline, sender);
		}

		[Fact]
		public void MoveToRegionThrowsOnNull()
		{
			var map = new Map();
			Assert.Throws<ArgumentNullException>(() => map.MoveToRegion(null!));
		}

		[Fact]
		public void MoveToRegionAnimatedThrowsOnNull()
		{
			var map = new Map();
			Assert.Throws<ArgumentNullException>(() => map.MoveToRegion(null!, true));
		}

		[Fact]
		public void MoveToRegionAnimatedOverloadExists()
		{
			var map = new Map();
			var span = new MapSpan(new Location(0, 0), 1, 1);

			// Should not throw - verifies the overload exists and is callable
			map.MoveToRegion(span, false);
			map.MoveToRegion(span, true);
		}

		[Fact]
		public void MoveToRegionRequestProperties()
		{
			var span = new MapSpan(new Location(10, 20), 1, 1);
			var request = new MoveToRegionRequest(span, true);

			Assert.Same(span, request.Region);
			Assert.True(request.Animated);

			var request2 = new MoveToRegionRequest(null, false);
			Assert.Null(request2.Region);
			Assert.False(request2.Animated);
		}

		[Fact]
		public void ShowInfoWindowDoesNotThrowWhenPinHasNoParent()
		{
			var pin = new Pin
			{
				Label = "Test",
				Location = new Location(0, 0)
			};

			// Should not throw even without a parent map
			pin.ShowInfoWindow();
		}

		[Fact]
		public void HideInfoWindowDoesNotThrowWhenPinHasNoParent()
		{
			var pin = new Pin
			{
				Label = "Test",
				Location = new Location(0, 0)
			};

			// Should not throw even without a parent map
			pin.HideInfoWindow();
		}

		[Fact]
		public void ShowInfoWindowOnPinAddedToMap()
		{
			var map = new Map();
			var pin = new Pin
			{
				Label = "Test",
				Location = new Location(47.6, -122.3)
			};
			map.Pins.Add(pin);

			// Pin has a parent now; without a handler it's a no-op but should not throw
			pin.ShowInfoWindow();
		}

		[Fact]
		public void HideInfoWindowOnPinAddedToMap()
		{
			var map = new Map();
			var pin = new Pin
			{
				Label = "Test",
				Location = new Location(47.6, -122.3)
			};
			map.Pins.Add(pin);

			// Pin has a parent now; without a handler it's a no-op but should not throw
			pin.HideInfoWindow();
		}

		[Fact]
		public void MapLongClickedEventFires()
		{
			var map = new Map();
			var expectedLocation = new Location(37.79752, -122.40183);

			bool eventFired = false;
			Location receivedLocation = null;
			map.MapLongClicked += (s, e) =>
			{
				eventFired = true;
				receivedLocation = e.Location;
			};

			((IMap)map).LongClicked(expectedLocation);

			Assert.True(eventFired);
			Assert.Equal(expectedLocation.Latitude, receivedLocation.Latitude);
			Assert.Equal(expectedLocation.Longitude, receivedLocation.Longitude);
		}

		[Fact]
		public void MapLongClickedEventSenderIsMap()
		{
			var map = new Map();

			object sender = null;
			map.MapLongClicked += (s, e) => sender = s;

			((IMap)map).LongClicked(new Location(37.79752, -122.40183));

			Assert.Same(map, sender);
		}

		[Fact]
		public void MapLongClickedEventArgsContainsLocation()
		{
			var map = new Map();
			var expectedLocation = new Location(47.6062, -122.3321);

			MapClickedEventArgs eventArgs = null;
			map.MapLongClicked += (s, e) => eventArgs = e;

			((IMap)map).LongClicked(expectedLocation);

			Assert.NotNull(eventArgs);
			Assert.NotNull(eventArgs.Location);
			Assert.Equal(expectedLocation.Latitude, eventArgs.Location.Latitude);
			Assert.Equal(expectedLocation.Longitude, eventArgs.Location.Longitude);
		}

		[Fact]
		public void IsClusteringEnabledDefaultValue()
		{
			var map = new Map();
			Assert.False(map.IsClusteringEnabled);
		}

		[Fact]
		public void IsClusteringEnabledCanBeSet()
		{
			var map = new Map();
			map.IsClusteringEnabled = true;
			Assert.True(map.IsClusteringEnabled);
		}

		[Fact]
		public void ClusteringIdentifierDefaultValue()
		{
			var pin = new Pin { Label = "Test" };
			Assert.Equal(Pin.DefaultClusteringIdentifier, pin.ClusteringIdentifier);
			Assert.Equal("maui_default_cluster", pin.ClusteringIdentifier);
		}

		[Fact]
		public void ClusteringIdentifierCanBeSet()
		{
			var pin = new Pin { Label = "Test", ClusteringIdentifier = "restaurants" };
			Assert.Equal("restaurants", pin.ClusteringIdentifier);
		}

		[Fact]
		public void ClusterClickedEventRaises()
		{
			var map = new Map();
			bool eventRaised = false;
			ClusterClickedEventArgs receivedArgs = null;

			map.ClusterClicked += (sender, args) =>
			{
				eventRaised = true;
				receivedArgs = args;
			};

			// Simulate cluster click via IMap interface
			var pins = new List<Pin>
			{
				new Pin { Label = "Pin 1", Location = new Location(0, 0) },
				new Pin { Label = "Pin 2", Location = new Location(0.1, 0.1) }
			};
			var location = new Location(0.05, 0.05);

			var handled = ((IMap)map).ClusterClicked(pins.Cast<IMapPin>().ToList(), location);

			Assert.True(eventRaised);
			Assert.NotNull(receivedArgs);
			Assert.Equal(2, receivedArgs.Pins.Count);
			Assert.Equal(location, receivedArgs.Location);
			Assert.False(handled); // Default is not handled
		}

		[Fact]
		public void ClusterClickedEventCanBeHandled()
		{
			var map = new Map();
			map.ClusterClicked += (sender, args) =>
			{
				args.Handled = true;
			};

			var pins = new List<Pin>
			{
				new Pin { Label = "Pin 1", Location = new Location(0, 0) }
			};
			var location = new Location(0.05, 0.05);

			var handled = ((IMap)map).ClusterClicked(pins.Cast<IMapPin>().ToList(), location);

			Assert.True(handled);
		}

		[Fact]
		public void ClusterClickedEventArgsContainsCorrectData()
		{
			var pins = new List<Pin>
			{
				new Pin { Label = "Pin 1", Location = new Location(1, 2), Address = "Address 1" },
				new Pin { Label = "Pin 2", Location = new Location(3, 4), Address = "Address 2" },
				new Pin { Label = "Pin 3", Location = new Location(5, 6), Address = "Address 3" }
			};
			var location = new Location(2, 3);

			var args = new ClusterClickedEventArgs(pins, location);

			Assert.Equal(3, args.Pins.Count);
			Assert.Equal("Pin 1", args.Pins[0].Label);
			Assert.Equal("Pin 2", args.Pins[1].Label);
			Assert.Equal("Pin 3", args.Pins[2].Label);
			Assert.Equal(2, args.Location.Latitude);
			Assert.Equal(3, args.Location.Longitude);
			Assert.False(args.Handled);
		}

		[Fact]
		public void PinClusteringIdentifierImplementsIMapPin()
		{
			var pin = new Pin { Label = "Test", ClusteringIdentifier = "custom_cluster" };
			IMapPin mapPin = pin;
			Assert.Equal("custom_cluster", mapPin.ClusteringIdentifier);
		}

		[Fact]
		public void MapClickedAndLongClickedCanCoexist()
		{
			var map = new Map();
			var clickLocation = new Location(37.7749, -122.4194);
			var longClickLocation = new Location(47.6062, -122.3321);

			bool clickFired = false;
			bool longClickFired = false;
			map.MapClicked += (s, e) => clickFired = true;
			map.MapLongClicked += (s, e) => longClickFired = true;

			// Fire click - only click handler should respond
			((IMap)map).Clicked(clickLocation);
			Assert.True(clickFired);
			Assert.False(longClickFired);

			// Reset and fire long click - only long click handler should respond
			clickFired = false;
			((IMap)map).LongClicked(longClickLocation);
			Assert.False(clickFired);
			Assert.True(longClickFired);
		}

		[Fact]
		public void MapLongClickedDoesNotFireWithoutHandler()
		{
			var map = new Map();
			
			// Should not throw when no handler is attached
			var exception = Record.Exception(() => ((IMap)map).LongClicked(new Location(37.7749, -122.4194)));
			
			Assert.Null(exception);
		}

		[Fact]
		public void MapLongClickedMultipleHandlersAllFire()
		{
			var map = new Map();
			var location = new Location(37.7749, -122.4194);

			int handler1Count = 0;
			int handler2Count = 0;
			map.MapLongClicked += (s, e) => handler1Count++;
			map.MapLongClicked += (s, e) => handler2Count++;

			((IMap)map).LongClicked(location);

			Assert.Equal(1, handler1Count);
			Assert.Equal(1, handler2Count);
		}

		[Fact]
		public void MapLongClickedHandlerCanBeRemoved()
		{
			var map = new Map();
			var location = new Location(37.7749, -122.4194);

			int fireCount = 0;
			EventHandler<MapClickedEventArgs> handler = (s, e) => fireCount++;
			
			map.MapLongClicked += handler;
			((IMap)map).LongClicked(location);
			Assert.Equal(1, fireCount);

			map.MapLongClicked -= handler;
			((IMap)map).LongClicked(location);
			Assert.Equal(1, fireCount); // Should still be 1, not 2
		}

		#region UserLocationChanged Tests

		[Fact]
		public void UserLocationChanged_EventRaisedOnLocationUpdate()
		{
			// Arrange
			var map = new Map();
			var eventRaised = false;
			Location receivedLocation = null;
			map.UserLocationChanged += (sender, args) =>
			{
				eventRaised = true;
				receivedLocation = args.Location;
			};

			var testLocation = new Location(37.7749, -122.4194); // San Francisco

			// Act
			((IMap)map).UserLocationUpdated(testLocation);

			// Assert
			Assert.True(eventRaised);
			Assert.NotNull(receivedLocation);
			Assert.Equal(37.7749, receivedLocation.Latitude);
			Assert.Equal(-122.4194, receivedLocation.Longitude);
		}

		[Fact]
		public void UserLocationChanged_SenderIsMap()
		{
			// Arrange
			var map = new Map();
			object sender = null;
			map.UserLocationChanged += (s, args) => sender = s;

			// Act
			((IMap)map).UserLocationUpdated(new Location(0, 0));

			// Assert
			Assert.Same(map, sender);
		}

		[Fact]
		public void LastUserLocation_IsNullByDefault()
		{
			// Arrange & Act
			var map = new Map();

			// Assert
			Assert.Null(map.LastUserLocation);
		}

		[Fact]
		public void LastUserLocation_UpdatedOnLocationUpdate()
		{
			// Arrange
			var map = new Map();
			var testLocation = new Location(51.5074, -0.1278); // London

			// Act
			((IMap)map).UserLocationUpdated(testLocation);

			// Assert
			Assert.NotNull(map.LastUserLocation);
			Assert.Equal(51.5074, map.LastUserLocation.Latitude);
			Assert.Equal(-0.1278, map.LastUserLocation.Longitude);
		}

		[Fact]
		public void LastUserLocation_UpdatedWithLatestLocation()
		{
			// Arrange
			var map = new Map();
			var firstLocation = new Location(40.7128, -74.0060); // NYC
			var secondLocation = new Location(34.0522, -118.2437); // LA

			// Act
			((IMap)map).UserLocationUpdated(firstLocation);
			((IMap)map).UserLocationUpdated(secondLocation);

			// Assert
			Assert.NotNull(map.LastUserLocation);
			Assert.Equal(34.0522, map.LastUserLocation.Latitude);
			Assert.Equal(-118.2437, map.LastUserLocation.Longitude);
		}

		[Fact]
		public void UserLocationChanged_MultipleSubscribers()
		{
			// Arrange
			var map = new Map();
			int eventCount = 0;
			map.UserLocationChanged += (s, e) => eventCount++;
			map.UserLocationChanged += (s, e) => eventCount++;

			// Act
			((IMap)map).UserLocationUpdated(new Location(0, 0));

			// Assert
			Assert.Equal(2, eventCount);
		}

		[Fact]
		public void IMap_LastUserLocation_ReturnsMapLastUserLocation()
		{
			// Arrange
			var map = new Map();
			var testLocation = new Location(48.8566, 2.3522); // Paris

			// Act
			((IMap)map).UserLocationUpdated(testLocation);
			var iMapLocation = ((IMap)map).LastUserLocation;

			// Assert
			Assert.NotNull(iMapLocation);
			Assert.Equal(map.LastUserLocation, iMapLocation);
		}

		#endregion

		[Fact]
		public void FromLocationsHandlesAntimeridianCrossing()
		{
			// Points near the antimeridian (179° and -179° should result in a small span, not 358°)
			var locations = new[]
			{
				new Location(0, 179),
				new Location(0, -179)
			};

			var span = MapSpan.FromLocations(locations);

			// The span should be small (around 2-3 degrees), not 358 degrees
			Assert.True(span.LongitudeDegrees < 10, $"Expected small longitude span for antimeridian crossing, got {span.LongitudeDegrees}");
		}

		[Fact]
		public void ClickedDoesNotThrowWithNoElements()
		{
			// Regression test for https://github.com/dotnet/maui/issues/34910
			// When tapping a Map with no MapElements (overlays), the iOS handler
			// would throw NullReferenceException because MKMapView.Overlays returns null
			var map = new Map();

			var exception = Record.Exception(() => ((IMap)map).Clicked(new Location(37.7749, -122.4194)));

			Assert.Null(exception);
		}

		[Fact]
		public void ClickedFiresEventWithNoElements()
		{
			// Verify MapClicked event fires correctly even with no map elements
			var map = new Map();
			var location = new Location(37.7749, -122.4194);
			MapClickedEventArgs eventArgs = null!;
			map.MapClicked += (s, e) => eventArgs = e;

			((IMap)map).Clicked(location);

			Assert.NotNull(eventArgs);
			Assert.Equal(location.Latitude, eventArgs.Location.Latitude);
			Assert.Equal(location.Longitude, eventArgs.Location.Longitude);
		}
	}
}
