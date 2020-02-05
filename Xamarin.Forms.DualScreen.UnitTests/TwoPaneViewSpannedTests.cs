using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.DualScreen.UnitTests
{
	[TestFixture]
	public class TwoPaneViewSpannedTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		TwoPaneView CreateTwoPaneView(IDualScreenService dualScreenService, View pane1 = null, View pane2 = null)
		{
			pane1 = pane1 ?? new BoxView();
			pane2 = pane2 ?? new BoxView();

			TwoPaneView view = new TwoPaneView(dualScreenService)
			{
				IsPlatformEnabled = true,
				Pane1 = pane1,
				Pane2 = pane2
			};

			view.MinTallModeHeight = 4000;
			view.MinWideModeWidth = 4000;

			if (pane1 != null)
				pane1.IsPlatformEnabled = true;

			if (pane2 != null)
				pane2.IsPlatformEnabled = true;

			view.Children[0].IsPlatformEnabled = true;
			view.Children[1].IsPlatformEnabled = true;

			return view;
		}

		[Test]
		public void BasicSpanTest()
		{
			var deviceInfo = new TestDeviceInfoPortrait();
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait(deviceInfo);
			var result = CreateTwoPaneView(testDualScreenService);

			result.Layout(new Rectangle(Point.Zero, deviceInfo.ScaledScreenSize));

			Assert.AreEqual(490, result.Children[0].Width);
			Assert.AreEqual(490, result.Children[1].Width);
			Assert.AreEqual(510, result.Children[1].X);

			Assert.AreEqual(490, result.Pane1.Width);
			Assert.AreEqual(490, result.Pane2.Width);

			Assert.AreEqual(deviceInfo.ScaledScreenSize.Height, result.Pane1.Height);
			Assert.AreEqual(deviceInfo.ScaledScreenSize.Height, result.Pane2.Height);
		}

		[Test]
		public void DeviceWithoutSpanSupport()
		{
			var deviceInfo = new TestDeviceInfoPortrait();
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait(deviceInfo);
			testDualScreenService.IsSpanned = false;
			testDualScreenService.SetHinge(Rectangle.Zero);
			var result = CreateTwoPaneView(testDualScreenService);

			result.Layout(new Rectangle(100, 100 ,200, 200));

			Assert.AreEqual(200, result.Children[0].Width);
			Assert.AreEqual(200, result.Children[0].Height);
		}

		[Test]
		public void LayoutShiftedOffCenterWideMode()
		{
			var deviceInfo = new TestDeviceInfoPortrait();
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait(deviceInfo);
			testDualScreenService.SetLocationOnScreen(new Point(400, 0));
			var result = CreateTwoPaneView(testDualScreenService);

			result.Layout(new Rectangle(400, 0, 500, deviceInfo.ScaledScreenSize.Height));

			Assert.AreEqual(90, result.Children[0].Width);
			Assert.AreEqual(400, result.Bounds.X);
			Assert.AreEqual(390, result.Children[1].Width);
			Assert.AreEqual(110, result.Children[1].X);
		}


		[Test]
		public void PortraitLayoutStartUnderneathHinge()
		{
			var deviceInfo = new TestDeviceInfoPortrait();
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait(deviceInfo);
			testDualScreenService.SetLocationOnScreen(new Point(500, 0));
			var result = CreateTwoPaneView(testDualScreenService);

			result.Layout(new Rectangle(500, 0, 400, deviceInfo.ScaledScreenSize.Height));

			Assert.AreEqual(390, result.Pane1.Width);
			Assert.AreEqual(10, result.Pane1.X);
			Assert.IsFalse(result.Children[1].IsVisible);
		}

		[Test]
		public void PortraitLayoutEndsUnderneathHinge()
		{
			var deviceInfo = new TestDeviceInfoPortrait();
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait(deviceInfo);
			testDualScreenService.SetLocationOnScreen(new Point(100, 0));
			var result = CreateTwoPaneView(testDualScreenService);

			result.Layout(new Rectangle(100, 0, 400, deviceInfo.ScaledScreenSize.Height));

			Assert.AreEqual(390, result.Pane1.Width);
			Assert.AreEqual(0, result.Pane1.X);
			Assert.IsFalse(result.Children[1].IsVisible);
		}


		[Test]
		public void LandscapeLayoutStartUnderneathHinge()
		{
			var deviceInfo = new TestDeviceInfoLandscape();
			var testDualScreenService = new TestDualScreenServiceLandscape(deviceInfo);
			testDualScreenService.SetLocationOnScreen(new Point(0, 500));
			var result = CreateTwoPaneView(testDualScreenService);

			result.Layout(new Rectangle(0, 500, deviceInfo.ScaledScreenSize.Width, 400));

			Assert.AreEqual(390, result.Pane1.Height);
			Assert.AreEqual(10, result.Pane1.Y);
			Assert.IsFalse(result.Children[1].IsVisible);
		}

		[Test]
		public void LandscapeLayoutEndsUnderneathHinge()
		{
			var deviceInfo = new TestDeviceInfoPortrait();
			var testDualScreenService = new TestDualScreenServiceLandscape(deviceInfo);
			testDualScreenService.SetLocationOnScreen(new Point(0, 100));
			var result = CreateTwoPaneView(testDualScreenService);

			result.Layout(new Rectangle(0, 100, deviceInfo.ScaledScreenSize.Width, 400));

			Assert.AreEqual(390, result.Pane1.Height);
			Assert.AreEqual(0, result.Pane1.Y);
			Assert.IsFalse(result.Children[1].IsVisible);
		}


		[Test]
		public void LayoutShiftedOffCenterTallMode()
		{
			var deviceInfo = new TestDeviceInfoLandscape();
			var testDualScreenService = new TestDualScreenServiceLandscape(deviceInfo);
			testDualScreenService.SetLocationOnScreen(new Point(0, 400));
			var result = CreateTwoPaneView(testDualScreenService);

			result.Layout(new Rectangle(0, 400, deviceInfo.ScaledScreenSize.Width, 500));

			Assert.AreEqual(90, result.Children[0].Height);
			Assert.AreEqual(400, result.Bounds.Y);
			Assert.AreEqual(390, result.Children[1].Height);
			Assert.AreEqual(110, result.Children[1].Y);
		}

		internal class TestDualScreenServiceLandscape : IDualScreenService
		{
			Point _location;
			Rectangle _hinge;
			public TestDualScreenServiceLandscape(DeviceInfo deviceInfo)
			{
				DeviceInfo = deviceInfo;
				_hinge = new Rectangle(0, 490, DeviceInfo.ScaledScreenSize.Width, 20);
				IsSpanned = true;
				IsLandscape = true;
				_location = Point.Zero;
			}

			public bool IsSpanned { get; set; }

			public bool IsLandscape { get; set; }

			public DeviceInfo DeviceInfo { get; set; }

			public event EventHandler OnScreenChanged;

			public void Dispose()
			{
			}

			public Rectangle GetHinge() => _hinge;

			public void SetHinge(Rectangle rectangle) => _hinge = rectangle;

			public Point? GetLocationOnScreen(VisualElement visualElement) => _location;

			public Point? SetLocationOnScreen(Point point) => _location = point;
		}

		internal class TestDualScreenServicePortrait : IDualScreenService
		{
			Point _location;
			Rectangle _hinge;
			public TestDualScreenServicePortrait(DeviceInfo deviceInfo)
			{
				DeviceInfo = deviceInfo;
				_hinge = new Rectangle(490, 0, 20, DeviceInfo.ScaledScreenSize.Height);
				IsSpanned = true;
				IsLandscape = false;
				_location = Point.Zero;
			}

			public bool IsSpanned { get; set; }

			public bool IsLandscape { get; set; }

			public DeviceInfo DeviceInfo { get; set; }

			public event EventHandler OnScreenChanged;

			public void Dispose()
			{
			}

			public Rectangle GetHinge() => _hinge;

			public void SetHinge(Rectangle rectangle) => _hinge = rectangle;

			public Point? GetLocationOnScreen(VisualElement visualElement) => _location;

			public Point? SetLocationOnScreen(Point point) => _location = point;
		}

		internal class TestDeviceInfoLandscape : DeviceInfo
		{
			public TestDeviceInfoLandscape()
			{
				CurrentOrientation = DeviceOrientation.Landscape;
			}

			public override Size PixelScreenSize
			{
				get { return new Size(1000, 2000); }
			}

			public override Size ScaledScreenSize
			{
				get { return new Size(500, 1000); }
			}

			public override double ScalingFactor
			{
				get { return 2; }
			}
		}

		internal class TestDeviceInfoPortrait : DeviceInfo
		{
			public TestDeviceInfoPortrait()
			{
				CurrentOrientation = DeviceOrientation.Portrait;
			}

			public override Size PixelScreenSize
			{
				get { return new Size(2000, 1000); }
			}

			public override Size ScaledScreenSize
			{
				get { return new Size(1000, 500); }
			}

			public override double ScalingFactor
			{
				get { return 2; }
			}
		}
	}
}