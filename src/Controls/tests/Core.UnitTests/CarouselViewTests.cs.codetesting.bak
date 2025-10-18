using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class CarouselViewTests : BaseTestFixture
	{
		public CarouselViewTests()
		{
			DeviceDisplay.SetCurrent(new MockDeviceDisplay());
		}

		[Fact]
		public void TestConstructorAndDefaults()
		{
			var carouselView = new CarouselView();
			Assert.Null(carouselView.ItemsSource);
			Assert.Null(carouselView.ItemTemplate);
			Assert.NotNull(carouselView.ItemsLayout);
			Assert.True(carouselView.Position == 0);
		}

		[Fact]
		public void TestPositionChangedCommand()
		{
			var source = new List<string> { "1", "2", "3" };
			var carouselView = new CarouselView
			{
				ItemsSource = source
			};

			int countFired = 0;
			carouselView.PositionChangedCommand = new Command(() =>
			{
				countFired = countFired + 1;
			});
			Assert.Same(source, carouselView.ItemsSource);
			carouselView.Position = 1;
			Assert.True(countFired == 1);
		}

		[Fact]
		public void TestPositionChangedEvent()
		{
			var gotoPosition = 1;
			var source = new List<string> { "1", "2", "3" };
			var carouselView = new CarouselView
			{
				ItemsSource = source
			};

			int countFired = 0;
			carouselView.PositionChanged += (s, e) =>
			{
				countFired += 1;
			};
			Assert.Same(source, carouselView.ItemsSource);
			carouselView.Position = gotoPosition;
			Assert.True(countFired == 1);
		}

		[Fact]
		public void TestCurrentItemChangedCommand()
		{
			var gotoPosition = 1;
			var source = new List<string> { "1", "2", "3" };
			var carouselView = new CarouselView
			{
				ItemsSource = source
			};

			int countFired = 0;
			carouselView.CurrentItemChangedCommand = new Command(() =>
			{
				countFired += 1;
			});
			Assert.Same(source, carouselView.ItemsSource);
			carouselView.CurrentItem = source[gotoPosition];
			Assert.True(countFired == 1);
		}

		[Fact]
		public void TestCurrentItemChangedEvent()
		{
			var gotoPosition = 1;
			var source = new List<string> { "1", "2", "3" };
			var carouselView = new CarouselView
			{
				ItemsSource = source
			};

			int countFired = 0;
			carouselView.CurrentItemChanged += (s, e) =>
			{
				countFired += 1;
			};
			Assert.Same(source, carouselView.ItemsSource);
			carouselView.CurrentItem = source[gotoPosition];
			Assert.True(countFired == 1);
		}

		[Fact]
		public void TestAddRemoveItems()
		{
			var source = new List<string>();

			var carouselView = new CarouselView
			{
				ItemsSource = source
			};

			source.Add("1");
			source.Add("2");

			carouselView.ScrollTo(1, position: ScrollToPosition.Center, animate: false);
			source.Remove("2");

			Assert.Equal(0, carouselView.Position);
		}
	}
}
