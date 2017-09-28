using System;
using NUnit.Framework;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class MapTests : BaseTestFixture
	{
		[Test]
		public void AddPin ()
		{
			var map = new Map ();

			var home = new Pin {
				Label = "Home",
				Position = new Position (88, 2),
				Type = PinType.Place,
				Address = "123 My Place"
			};

			map.Pins.Add (home);

			Assert.AreEqual (map.Pins.Count, 1);
			Assert.AreEqual (map.Pins[0].Label, "Home");
			var mall = new Pin {
				Label = "Mall",
				Position = new Position (-12, -67),
				Type = PinType.Place,
				Address = "123 Fun"
			};

			map.Pins.Add (mall);

			Assert.AreEqual (map.Pins.Count, 2);
			Assert.AreEqual (map.Pins[1].Position.Latitude, -12);
		}

		[Test]
		public void AddPinWithoutName ()
		{
			var map = new Map ();
			var noNamePin = new Pin {
				Position = new Position (50, 50),
				Type = PinType.Generic,
				Address = "123 Fun"
			};

			var exception = Assert.Throws<ArgumentException> (() => map.Pins.Add (noNamePin));
			Assert.That (exception.Message, Is.EqualTo ("Pin must have a Label to be added to a map"));
		}

		[Test]
		public void AddPinWithoutAddress ()
		{
			var map = new Map ();
			var noAddressPin = new Pin {
				Position = new Position (37.9, -20.87),
				Label = "I have no address",
				Type = PinType.SearchResult
			};

			map.Pins.Add (noAddressPin);
			Assert.AreEqual (map.Pins.Count, 1);
			Assert.AreEqual (map.Pins[0].Label, "I have no address");
			Assert.AreEqual (map.Pins[0].Address, null);
		}

		[Test]
		public void Constructor ()
		{
			var center = new Position (15.5, 176);
			var span = new MapSpan (center, 1, 2);
			var map = new Map (span);

			Assert.AreEqual (1, map.LastMoveToRegion.LatitudeDegrees);
			Assert.AreEqual (2, map.LastMoveToRegion.LongitudeDegrees);
			var position = new Position (15.5, 176);
			Assert.AreEqual (position, map.LastMoveToRegion.Center);
		}

		[Test]
		public void RemovePin ()
		{
			var map = new Map ();
			var genericPlace = new Pin {
				Label = "Generic",
				Position = new Position (-12, -67),
				Type = PinType.Generic,
				Address = "XXX"
			};

			var mall = new Pin {
				Label = "Mall",
				Position = new Position (-29, -87),
				Type = PinType.Place,
				Address = "123 Fun"
			};

			map.Pins.Add (genericPlace);
			Assert.AreEqual (map.Pins.Count, 1);

			map.Pins.Add (mall);
			Assert.AreEqual (map.Pins.Count, 2);

			map.Pins.Remove (genericPlace);
			Assert.AreEqual (map.Pins.Count, 1);

			Assert.True (map.Pins.Contains (mall));
			Assert.False (map.Pins.Contains (genericPlace));
		}

		[Test]
		public void VisibleRegion ()
		{
			var map = new Map (new MapSpan (new Position (), 0, 0));
			map.MoveToRegion (new MapSpan (new Position (1, 2), 3, 4));
			Assert.AreEqual (null, map.VisibleRegion);

			bool signaled = false;
			MessagingCenter.Subscribe<Map, MapSpan> (this, "MapMoveToRegion", (s, a) => {
				signaled = true;
				map.SetVisibleRegion(a);
			}, map);

			map.MoveToRegion (new MapSpan (new Position (1, 2), 3, 4));
			Assert.AreEqual (new MapSpan (new Position (1, 2), 3, 4), map.LastMoveToRegion);
			Assert.True (signaled);
		}

		[Test]
		public void VisibleRegionDoubleSet ()
		{
			var map = new Map ();

			bool signaled = false;
			map.PropertyChanged += (sender, args) => {
				if (args.PropertyName == "VisibleRegion")
					signaled = true;
			};

			map.SetVisibleRegion(map.VisibleRegion);

			Assert.False (signaled);
		}
	}
}
