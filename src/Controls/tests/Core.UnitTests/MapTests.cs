using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls.Maps;
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
				Position = new Position(88, 2),
				Type = PinType.Place,
				Address = "123 My Place"
			};

			map.Pins.Add(home);

			Assert.Equal(1, map.Pins.Count);
			Assert.Equal("Home", map.Pins[0].Label);
			var mall = new Pin
			{
				Label = "Mall",
				Position = new Position(-12, -67),
				Type = PinType.Place,
				Address = "123 Fun"
			};

			map.Pins.Add(mall);

			Assert.Equal(2, map.Pins.Count);
			Assert.Equal(map.Pins[1].Position.Latitude, -12);
		}

		[Fact]
		public void AddPinWithoutName()
		{
			var map = new Map();
			var noNamePin = new Pin
			{
				Position = new Position(50, 50),
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
				Position = new Position(37.9, -20.87),
				Label = "I have no address",
				Type = PinType.SearchResult
			};

			map.Pins.Add(noAddressPin);
			Assert.Equal(1, map.Pins.Count);
			Assert.Equal("I have no address", map.Pins[0].Label);
			Assert.Null(map.Pins[0].Address);
		}

		[Fact]
		public void Constructor()
		{
			var center = new Position(15.5, 176);
			var span = new MapSpan(center, 1, 2);
			var map = new Map(span);

			Assert.Equal(1, map.LastMoveToRegion.LatitudeDegrees);
			Assert.Equal(2, map.LastMoveToRegion.LongitudeDegrees);
			var position = new Position(15.5, 176);
			Assert.Equal(position, map.LastMoveToRegion.Center);
		}

		[Fact]
		public void RemovePin()
		{
			var map = new Map();
			var genericPlace = new Pin
			{
				Label = "Generic",
				Position = new Position(-12, -67),
				Type = PinType.Generic,
				Address = "XXX"
			};

			var mall = new Pin
			{
				Label = "Mall",
				Position = new Position(-29, -87),
				Type = PinType.Place,
				Address = "123 Fun"
			};

			map.Pins.Add(genericPlace);
			Assert.Equal(1, map.Pins.Count);

			map.Pins.Add(mall);
			Assert.Equal(2, map.Pins.Count);

			map.Pins.Remove(genericPlace);
			Assert.Equal(1, map.Pins.Count);

			Assert.True(map.Pins.Contains(mall));
			Assert.False(map.Pins.Contains(genericPlace));
		}

		[Fact]
		public void VisibleRegion()
		{
			var map = new Map(new MapSpan(new Position(), 0, 0));
			map.MoveToRegion(new MapSpan(new Position(1, 2), 3, 4));
			Assert.Null(map.VisibleRegion);

			bool signaled = false;
			MessagingCenter.Subscribe<Map, MapSpan>(this, "MapMoveToRegion", (s, a) =>
			{
				signaled = true;
				map.SetVisibleRegion(a);
			}, map);

			map.MoveToRegion(new MapSpan(new Position(1, 2), 3, 4));
			Assert.Equal(new MapSpan(new Position(1, 2), 3, 4), map.LastMoveToRegion);
			Assert.True(signaled);
		}

		[Fact]
		public void VisibleRegionDoubleSet()
		{
			var map = new Map();

			bool signaled = false;
			map.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "VisibleRegion")
					signaled = true;
			};

			map.SetVisibleRegion(map.VisibleRegion);

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
			foreach (Pin pin in map.Pins)
			{
				Assert.True(pin.Address == "Address");
			}

			map.ItemTemplate = GetItemTemplate("Address 2");
			Assert.True(IsMapWithItemsSource(itemsSource, map));
			foreach (Pin pin in map.Pins)
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
			foreach (Pin pin in map.Pins)
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
			foreach (Pin pin in map.Pins)
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

		[Fact(Skip ="https://github.com/dotnet/maui/issues/1524")]
		public void ElementIsGarbageCollectedAfterItsRemoved()
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
			itemsSource = null;

			// Remove map from container
			var pageRoot = new Grid();
			pageRoot.Children.Add(map);
			var page = new ContentPage() { Content = pageRoot };

			var weakReference = new WeakReference(map);
			pageRoot.Children.Remove(map);
			map = null;

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.False(weakReference.IsAlive);
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
				if (!map.Pins.Any(p => Equals(item, p.BindingContext)))
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
				Position = new Position()
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
	}
}
