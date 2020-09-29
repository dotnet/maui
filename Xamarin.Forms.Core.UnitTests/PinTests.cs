using NUnit.Framework;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class PinTests : BaseTestFixture
	{
		[Test]
		public void Constructor()
		{
			Pin pin = new Pin
			{
				Type = PinType.SavedPin,
				Position = new Position(-92, 178),
				Label = "My Desktop",
				Address = "123 Hello World Street"
			};

			Assert.AreEqual(pin.Type, PinType.SavedPin);
			Assert.AreEqual(pin.Position.Latitude, -90);
			Assert.AreEqual(pin.Label, "My Desktop");
			Assert.AreEqual(pin.Address, "123 Hello World Street");
		}

		[Test]
		public void Equals()
		{
			Pin pin1 = new Pin();
			Pin pin2 = new Pin();
			Pin pin3 = new Pin
			{
				Type = PinType.Place,
				Position = new Position(12, -24),
				Label = "Test",
				Address = "123 Test street"
			};

			Pin pin4 = new Pin
			{
				Type = PinType.Place,
				Position = new Position(12, -24),
				Label = "Test",
				Address = "123 Test street"
			};

			Assert.True(pin1.Equals(pin2));
			Assert.True(pin3.Equals(pin4));
			Assert.False(pin1.Equals(pin3));
		}

		[Test]
		public void EqualsOp()
		{
			var pin1 = new Pin
			{
				Type = PinType.Place,
				Position = new Position(12, -24),
				Label = "Test",
				Address = "123 Test street"
			};

			var pin2 = new Pin
			{
				Type = PinType.Place,
				Position = new Position(12, -24),
				Label = "Test",
				Address = "123 Test street"
			};

			Assert.True(pin1 == pin2);
		}

		[Test]
		public void InEqualsOp()
		{
			var pin1 = new Pin
			{
				Type = PinType.Place,
				Position = new Position(11.9, -24),
				Label = "Test",
				Address = "123 Test street"
			};

			var pin2 = new Pin
			{
				Type = PinType.Place,
				Position = new Position(12, -24),
				Label = "Test",
				Address = "123 Test street"
			};

			Assert.True(pin1 != pin2);
		}

		[Test]
		public void Label()
		{
			var pin = new Pin
			{
				Label = "OriginalLabel"
			};

			bool signaled = false;
			pin.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Label")
					signaled = true;
			};

			pin.Label = "Should Signal";

			Assert.True(signaled);
		}
	}
}