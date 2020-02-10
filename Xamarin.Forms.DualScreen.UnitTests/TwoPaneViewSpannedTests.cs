using NUnit.Framework;

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

		TwoPaneView CreateTwoPaneView(TestDualScreenService dualScreenService, View pane1 = null, View pane2 = null)
		{
			pane1 = pane1 ?? new BoxView();
			pane2 = pane2 ?? new BoxView();
			dualScreenService.IsSpanned = true;
			
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
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait();
			var result = CreateTwoPaneView(testDualScreenService);

			result.Layout(new Rectangle(Point.Zero, testDualScreenService.DeviceInfo.ScaledScreenSize));

			Assert.AreEqual(490, result.Children[0].Width);
			Assert.AreEqual(490, result.Children[1].Width);
			Assert.AreEqual(510, result.Children[1].X);

			Assert.AreEqual(490, result.Pane1.Width);
			Assert.AreEqual(490, result.Pane2.Width);

			Assert.AreEqual(testDualScreenService.DeviceInfo.ScaledScreenSize.Height, result.Pane1.Height);
			Assert.AreEqual(testDualScreenService.DeviceInfo.ScaledScreenSize.Height, result.Pane2.Height);
		}

		[Test]
		public void LayoutShiftedOffCenterWideMode()
		{
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait();
			testDualScreenService.SetLocationOnScreen(new Point(400, 0));
			var result = CreateTwoPaneView(testDualScreenService);

			result.Layout(new Rectangle(400, 0, 500, testDualScreenService.DeviceInfo.ScaledScreenSize.Height));

			Assert.AreEqual(90, result.Children[0].Width);
			Assert.AreEqual(400, result.Bounds.X);
			Assert.AreEqual(390, result.Children[1].Width);
			Assert.AreEqual(110, result.Children[1].X);
		}


		[Test]
		public void LayoutShiftedOffCenterTallMode()
		{
			var testDualScreenService = new TestDualScreenServiceLandscape();
			testDualScreenService.SetLocationOnScreen(new Point(0, 400));
			var result = CreateTwoPaneView(testDualScreenService);

			result.Layout(new Rectangle(0, 400, testDualScreenService.DeviceInfo.ScaledScreenSize.Width, 500));

			Assert.AreEqual(90, result.Children[0].Height);
			Assert.AreEqual(400, result.Bounds.Y);
			Assert.AreEqual(390, result.Children[1].Height);
			Assert.AreEqual(110, result.Children[1].Y);
		}



		[Test]
		public void PortraitLayoutStartUnderneathHinge()
		{
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait();
			testDualScreenService.SetLocationOnScreen(new Point(500, 0));
			var result = CreateTwoPaneView(dualScreenService: testDualScreenService);

			result.Layout(new Rectangle(500, 0, 400, testDualScreenService.DeviceInfo.ScaledScreenSize.Height));

			Assert.AreEqual(390, result.Pane1.Width);
			Assert.AreEqual(10, result.Pane1.X);
			Assert.IsFalse(result.Children[1].IsVisible);
		}

		[Test]
		public void PortraitLayoutEndsUnderneathHinge()
		{
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait();
			testDualScreenService.SetLocationOnScreen(new Point(100, 0));
			var result = CreateTwoPaneView(dualScreenService: testDualScreenService);

			result.Layout(new Rectangle(100, 0, 400, testDualScreenService.DeviceInfo.ScaledScreenSize.Height));

			Assert.AreEqual(390, result.Pane1.Width);
			Assert.AreEqual(0, result.Pane1.X);
			Assert.IsFalse(result.Children[1].IsVisible);
		}


		[Test]
		public void LandscapeLayoutStartUnderneathHinge()
		{
			var testDualScreenService = new TestDualScreenServiceLandscape();
			testDualScreenService.SetLocationOnScreen(new Point(0, 500));
			var result = CreateTwoPaneView(dualScreenService: testDualScreenService);
			result.Layout(new Rectangle(0, 500, testDualScreenService.DeviceInfo.ScaledScreenSize.Width, 400));

			Assert.AreEqual(390, result.Pane1.Height);
			Assert.AreEqual(10, result.Pane1.Y);
			Assert.IsFalse(result.Children[1].IsVisible);
		}

		[Test]
		public void LandscapeLayoutEndsUnderneathHinge()
		{
			var testDualScreenService = new TestDualScreenServiceLandscape();
			testDualScreenService.SetLocationOnScreen(new Point(0, 100));
			var result = CreateTwoPaneView(dualScreenService: testDualScreenService);

			result.Layout(new Rectangle(0, 100, testDualScreenService.DeviceInfo.ScaledScreenSize.Width, 400));

			Assert.AreEqual(390, result.Pane1.Height);
			Assert.AreEqual(0, result.Pane1.Y);
			Assert.IsFalse(result.Children[1].IsVisible);
		}


		[Test]
		public void NestedTwoPaneViews()
		{
			Grid grid = new Grid();
			TestDualScreenServicePortrait testDualScreenServicePortrait = new TestDualScreenServicePortrait();
			var twoPaneViewNested = CreateTwoPaneView(testDualScreenServicePortrait);
			testDualScreenServicePortrait.SetLocationOnScreen(new Point(400, 0));
			twoPaneViewNested.HeightRequest = 200;
			twoPaneViewNested.WidthRequest = 200;
			twoPaneViewNested.HorizontalOptions = LayoutOptions.Center;
			twoPaneViewNested.VerticalOptions = LayoutOptions.Center;
			twoPaneViewNested.MinTallModeHeight = double.MaxValue;
			twoPaneViewNested.MinWideModeWidth = double.MaxValue;

			grid.Children.Add(twoPaneViewNested);
			grid.Layout(new Rectangle(Point.Zero, testDualScreenServicePortrait.DeviceInfo.ScaledScreenSize));

			Assert.AreEqual(twoPaneViewNested.Mode, TwoPaneViewMode.Wide);

			Assert.AreEqual(90, twoPaneViewNested.Pane1.Width);
			Assert.AreEqual(90, twoPaneViewNested.Pane2.Width);
			Assert.AreEqual(200, twoPaneViewNested.Pane1.Height);
			Assert.AreEqual(200, twoPaneViewNested.Pane2.Height);

			Assert.AreEqual(0, twoPaneViewNested.Pane1.X);
			Assert.AreEqual(0, twoPaneViewNested.Pane2.X);
		}

		[Test]
		public void SpanningBoundsLandscape()
		{
			var testDualScreenService = new TestDualScreenServiceLandscape();
			testDualScreenService.IsSpanned = true;
			testDualScreenService.SetLocationOnScreen(new Point(0, 400));
			var result = new StackLayout() { IsPlatformEnabled = true };

			result.Layout(new Rectangle(0, 400, testDualScreenService.ScaledScreenSize.Width, 200));
			DualScreenInfo info = new DualScreenInfo(result, testDualScreenService);


			var top = info.SpanningBounds[0];

			Assert.AreEqual(0, top.X);
			Assert.AreEqual(0, top.Y);
			Assert.AreEqual(testDualScreenService.ScaledScreenSize.Width, top.Width);
			Assert.AreEqual(90, top.Height);

			var bottom = info.SpanningBounds[1];
			Assert.AreEqual(0, bottom.X);
			Assert.AreEqual(110, bottom.Y);
			Assert.AreEqual(testDualScreenService.ScaledScreenSize.Width, bottom.Width);
			Assert.AreEqual(90, bottom.Height);
		}

		[Test]
		public void SpanningBoundsPortrait()
		{
			var testDualScreenService = new TestDualScreenServicePortrait();
			testDualScreenService.IsSpanned = true;
			testDualScreenService.SetLocationOnScreen(new Point(400, 0));
			var result = new StackLayout() { IsPlatformEnabled = true };

			result.Layout(new Rectangle(400, 0, 200, testDualScreenService.ScaledScreenSize.Height));
			DualScreenInfo info = new DualScreenInfo(result, testDualScreenService);


			var left = info.SpanningBounds[0];

			Assert.AreEqual(0, left.X);
			Assert.AreEqual(0, left.Y);
			Assert.AreEqual(90, left.Width);
			Assert.AreEqual(testDualScreenService.ScaledScreenSize.Height, left.Height);

			var right = info.SpanningBounds[1];
			Assert.AreEqual(110, right.X);
			Assert.AreEqual(0, right.Y);
			Assert.AreEqual(90, right.Width);
			Assert.AreEqual(testDualScreenService.ScaledScreenSize.Height, right.Height);
		}
	}
}