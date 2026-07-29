using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class PinTests : BaseTestFixture
	{
		[Fact]
		public void Constructor()
		{
			Pin pin = new Pin
			{
				Type = PinType.SavedPin,
				Location = new Location(-90, 178),
				Label = "My Desktop",
				Address = "123 Hello World Street"
			};

			Assert.Equal(PinType.SavedPin, pin.Type);
			Assert.Equal(-90, pin.Location.Latitude);
			Assert.Equal(178, pin.Location.Longitude);
			Assert.Equal("My Desktop", pin.Label);
			Assert.Equal("123 Hello World Street", pin.Address);
		}

		[Fact]
		public void EqualsTest()
		{
			Pin pin1 = new Pin();
			Pin pin2 = new Pin();
			Pin pin3 = new Pin
			{
				Type = PinType.Place,
				Location = new Location(12, -24),
				Label = "Test",
				Address = "123 Test street"
			};

			Pin pin4 = new Pin
			{
				Type = PinType.Place,
				Location = new Location(12, -24),
				Label = "Test",
				Address = "123 Test street"
			};

			Assert.True(pin1.Equals(pin2));
			Assert.True(pin3.Equals(pin4));
			Assert.False(pin1.Equals(pin3));
		}

		[Fact]
		public void EqualsOp()
		{
			var pin1 = new Pin
			{
				Type = PinType.Place,
				Location = new Location(12, -24),
				Label = "Test",
				Address = "123 Test street"
			};

			var pin2 = new Pin
			{
				Type = PinType.Place,
				Location = new Location(12, -24),
				Label = "Test",
				Address = "123 Test street"
			};

			Assert.True(pin1 == pin2);
		}

		[Fact]
		public void InEqualsOp()
		{
			var pin1 = new Pin
			{
				Type = PinType.Place,
				Location = new Location(11.9, -24),
				Label = "Test",
				Address = "123 Test street"
			};

			var pin2 = new Pin
			{
				Type = PinType.Place,
				Location = new Location(12, -24),
				Label = "Test",
				Address = "123 Test street"
			};

			Assert.True(pin1 != pin2);
		}

		[Fact]
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

		[Fact]
		public void ImageSourceDefaultsToNull()
		{
			var pin = new Pin();
			Assert.Null(pin.ImageSource);
		}

		[Fact]
		public void ImageSourceCanBeSet()
		{
			var pin = new Pin
			{
				ImageSource = ImageSource.FromFile("test.png")
			};

			Assert.NotNull(pin.ImageSource);
			Assert.IsType<FileImageSource>(pin.ImageSource);
		}

		[Fact]
		public void ImageSourcePropertyChanged()
		{
			var pin = new Pin();

			bool signaled = false;
			pin.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "ImageSource")
					signaled = true;
			};

			pin.ImageSource = ImageSource.FromFile("test.png");

			Assert.True(signaled);
		}

		[Fact]
		public void ImageSourceBindableProperty()
		{
			var pin = new Pin();
			pin.SetValue(Pin.ImageSourceProperty, ImageSource.FromFile("bound.png"));

			Assert.NotNull(pin.ImageSource);
			Assert.IsType<FileImageSource>(pin.ImageSource);
		}

		[Fact]
		public void IMapPinImageSourceReturnsValue()
		{
			var pin = new Pin
			{
				ImageSource = ImageSource.FromFile("test.png")
			};

			// Access through interface to test explicit implementation
			var mapPin = (Microsoft.Maui.Maps.IMapPin)pin;
			Assert.NotNull(mapPin.ImageSource);
		}
	}
}
