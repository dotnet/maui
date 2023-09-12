using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
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

		[Fact]
		public async Task ElementIsGarbageCollectedAfterItsRemoved()
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

			await Task.Yield();
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
	}
}
