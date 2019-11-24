using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	public class CarouselViewTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			Device.SetFlags(new List<string> { ExperimentalFlags.CarouselViewExperimental });
			base.Setup();
			var mockDeviceInfo = new TestDeviceInfo();
			Device.Info = mockDeviceInfo;
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.Info = null;
		}

		[Test]
		public void TestConstructorAndDefaults()
		{
			var carouselView = new CarouselView();
			Assert.IsNull(carouselView.ItemsSource);
			Assert.IsNull(carouselView.ItemTemplate);
			Assert.IsNotNull(carouselView.ItemsLayout);
			Assert.IsTrue(carouselView.Position == 0);
		}

		[Test]
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
			Assert.AreSame(source, carouselView.ItemsSource);
			carouselView.Position = 1;
			Assert.IsTrue(countFired == 1);
		}

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
			Assert.AreSame(source, carouselView.ItemsSource);
			carouselView.Position = gotoPosition;
			Assert.IsTrue(countFired == 1);
		}

		[Test]
		public void TestCurrentItemChangesPosition()
		{
			var gotoPosition = 1;
			var source = new List<string> { "1", "2", "3" };
			var carouselView = new CarouselView
			{
				ItemsSource = source
			};

			int countFired = 0;
			carouselView.PositionChangedCommand = new Command(() =>
			{
				countFired += 1;
			});
			Assert.AreSame(source, carouselView.ItemsSource);
			carouselView.CurrentItem = source[gotoPosition];
			Assert.AreEqual(gotoPosition, carouselView.Position);
		}

		[Test]
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
			Assert.AreSame(source, carouselView.ItemsSource);
			carouselView.CurrentItem = source[gotoPosition];
			Assert.IsTrue(countFired == 1);
		}

		[Test]
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
			Assert.AreSame(source, carouselView.ItemsSource);
			carouselView.CurrentItem = source[gotoPosition];
			Assert.IsTrue(countFired == 1);
		}

		// TODO when Scrolled is implemented
		//[Test]
		//public void TestScrolled()
		//{
		//	var scrollPosition = 100;
		//	var scrollDirection = ScrollDirection.Down;
		//	var scrolledDirection = ScrollDirection.Up;
		//	var source = new List<string> { "1", "2", "3" };
		//	var carouselView = new CarouselView
		//	{
		//		ItemsSource = source
		//	};
		//	int countFired = 0;
		//	carouselView.Scrolled += (s, e) =>
		//	{
		//		countFired += 1;
		//		scrolledDirection = e.Direction;
		//	};
		//	carouselView.SendScrolled(scrollPosition, scrollDirection);
		//	Assert.IsTrue(countFired == 1);
		//	Assert.IsTrue(scrollDirection == scrolledDirection);
		//}
	}
}
