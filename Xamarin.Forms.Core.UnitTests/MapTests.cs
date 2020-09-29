using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class MapTests : BaseTestFixture
	{
		[Test]
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

			Assert.AreEqual(map.Pins.Count, 1);
			Assert.AreEqual(map.Pins[0].Label, "Home");
			var mall = new Pin
			{
				Label = "Mall",
				Position = new Position(-12, -67),
				Type = PinType.Place,
				Address = "123 Fun"
			};

			map.Pins.Add(mall);

			Assert.AreEqual(map.Pins.Count, 2);
			Assert.AreEqual(map.Pins[1].Position.Latitude, -12);
		}

		[Test]
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
			Assert.That(exception.Message, Is.EqualTo("Pin must have a Label to be added to a map"));
		}

		[Test]
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
			Assert.AreEqual(map.Pins.Count, 1);
			Assert.AreEqual(map.Pins[0].Label, "I have no address");
			Assert.AreEqual(map.Pins[0].Address, null);
		}

		[Test]
		public void Constructor()
		{
			var center = new Position(15.5, 176);
			var span = new MapSpan(center, 1, 2);
			var map = new Map(span);

			Assert.AreEqual(1, map.LastMoveToRegion.LatitudeDegrees);
			Assert.AreEqual(2, map.LastMoveToRegion.LongitudeDegrees);
			var position = new Position(15.5, 176);
			Assert.AreEqual(position, map.LastMoveToRegion.Center);
		}

		[Test]
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
			Assert.AreEqual(map.Pins.Count, 1);

			map.Pins.Add(mall);
			Assert.AreEqual(map.Pins.Count, 2);

			map.Pins.Remove(genericPlace);
			Assert.AreEqual(map.Pins.Count, 1);

			Assert.True(map.Pins.Contains(mall));
			Assert.False(map.Pins.Contains(genericPlace));
		}

		[Test]
		public void VisibleRegion()
		{
			var map = new Map(new MapSpan(new Position(), 0, 0));
			map.MoveToRegion(new MapSpan(new Position(1, 2), 3, 4));
			Assert.AreEqual(null, map.VisibleRegion);

			bool signaled = false;
			MessagingCenter.Subscribe<Map, MapSpan>(this, "MapMoveToRegion", (s, a) =>
			{
				signaled = true;
				map.SetVisibleRegion(a);
			}, map);

			map.MoveToRegion(new MapSpan(new Position(1, 2), 3, 4));
			Assert.AreEqual(new MapSpan(new Position(1, 2), 3, 4), map.LastMoveToRegion);
			Assert.True(signaled);
		}

		[Test]
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

		[Test]
		public void TracksEmpty()
		{
			var map = new Map();

			var itemsSource = new ObservableCollection<int>();
			map.ItemsSource = itemsSource;
			map.ItemTemplate = new DataTemplate();

			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
		}

		[Test]
		public void TracksAdd()
		{
			var itemsSource = new ObservableCollection<int>();

			var map = new Map()
			{
				ItemsSource = itemsSource,
				ItemTemplate = GetItemTemplate()
			};

			itemsSource.Add(1);
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
		}

		[Test]
		public void TracksInsert()
		{
			var itemsSource = new ObservableCollection<int>();

			var map = new Map()
			{
				ItemsSource = itemsSource,
				ItemTemplate = GetItemTemplate()
			};

			itemsSource.Insert(0, 1);
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
		}

		[Test]
		public void TracksRemove()
		{
			var itemsSource = new ObservableCollection<int>() { 0, 1 };

			var map = new Map()
			{
				ItemsSource = itemsSource,
				ItemTemplate = GetItemTemplate()
			};

			itemsSource.RemoveAt(0);
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));

			itemsSource.Remove(1);
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
		}

		[Test]
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
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
		}

		[Test]
		public void ItemMove()
		{
			var itemsSource = new ObservableCollection<int>() { 0, 1 };

			var map = new Map()
			{
				ItemsSource = itemsSource,
				ItemTemplate = GetItemTemplate()
			};

			itemsSource.Move(0, 1);
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));

			itemsSource.Move(1, 0);
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
		}

		[Test]
		public void TracksClear()
		{
			var itemsSource = new ObservableCollection<int>() { 0, 1 };

			var map = new Map()
			{
				ItemsSource = itemsSource,
				ItemTemplate = GetItemTemplate()
			};

			itemsSource.Clear();
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
		}

		[Test]
		public void TracksNull()
		{
			var map = new Map()
			{
				ItemTemplate = GetItemTemplate()
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			map.ItemsSource = itemsSource;
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));

			itemsSource = null;
			map.ItemsSource = itemsSource;
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
		}

		[Test]
		public void TracksItemTemplate()
		{
			var map = new Map()
			{
				ItemTemplate = GetItemTemplate()
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 3));
			map.ItemsSource = itemsSource;
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
			foreach (Pin pin in map.Pins)
			{
				Assert.IsTrue(pin.Address == "Address");
			}

			map.ItemTemplate = GetItemTemplate("Address 2");
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
			foreach (Pin pin in map.Pins)
			{
				Assert.IsTrue(pin.Address == "Address 2");
			}
		}

		[Test]
		public void ItemTemplateSelectorIsSet()
		{
			var map = new Map();

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 3));
			map.ItemsSource = itemsSource;
			map.ItemTemplateSelector = new TestDataTemplateSelector("Address 2");

			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
			foreach (Pin pin in map.Pins)
			{
				Assert.IsTrue(pin.Address == "Address 2");
			}
		}

		[Test]
		public void ItemTemplateTakesPrecendenceOverItemTemplateSelector()
		{
			var map = new Map();

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			map.ItemsSource = itemsSource;
			map.ItemTemplate = GetItemTemplate("Address 1");
			map.ItemTemplateSelector = new TestDataTemplateSelector("Address 2");

			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
			foreach (Pin pin in map.Pins)
			{
				Assert.AreEqual(pin.Address, "Address 1");
			}
		}

		[Test]
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
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
		}

		[Test]
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
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
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

			Assert.IsFalse(weakReference.IsAlive);
		}

		[Test]
		public void ThrowsExceptionOnUsingDataTemplateSelectorForItemTemplate()
		{
			var map = new Map();

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			map.ItemsSource = itemsSource;

			Assert.Throws(typeof(NotSupportedException), () => map.ItemTemplate = GetDataTemplateSelector());
		}

		[Test]
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
			Assert.IsTrue(itemsSource.Count() == 11);
		}

		[Test]
		public void WorksWithNullItems()
		{
			var map = new Map()
			{
				ItemTemplate = GetItemTemplate()
			};

			var itemsSource = new ObservableCollection<int?>(Enumerable.Range(0, 10).Cast<int?>());
			itemsSource.Add(null);
			map.ItemsSource = itemsSource;
			Assert.IsTrue(IsMapWithItemsSource(itemsSource, map));
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