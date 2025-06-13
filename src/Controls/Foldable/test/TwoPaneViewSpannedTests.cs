using Xunit;

namespace Microsoft.Maui.Controls.Foldable.UnitTests
{
	// [TestFixture] - removed for xUnit
	public class TwoPaneViewSpannedTests : BaseTestFixture
	{
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

		[Fact]
		public void BasicSpanTest()
		{
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait();
			var result = CreateTwoPaneView(testDualScreenService);

			result.Layout(new Rectangle(Point.Zero, testDualScreenService.DeviceInfo.ScaledScreenSize));

			Assert.Equal(490, result.Children[0].Width);
			Assert.Equal(490, result.Children[1].Width);
			Assert.Equal(510, result.Children[1].X);

			Assert.Equal(490, result.Pane1.Width);
			Assert.Equal(490, result.Pane2.Width);

			Assert.Equal(testDualScreenService.DeviceInfo.ScaledScreenSize.Height, result.Pane1.Height);
			Assert.Equal(testDualScreenService.DeviceInfo.ScaledScreenSize.Height, result.Pane2.Height);
		}

		[Fact]
		public void LayoutShiftedOffCenterWideMode()
		{
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait();
			testDualScreenService.SetLocationOnScreen(new Point(400, 0));
			var result = CreateTwoPaneView(testDualScreenService);

			result.Layout(new Rectangle(400, 0, 500, testDualScreenService.DeviceInfo.ScaledScreenSize.Height));

			Assert.Equal(90, result.Children[0].Width);
			Assert.Equal(400, result.Bounds.X);
			Assert.Equal(390, result.Children[1].Width);
			Assert.Equal(110, result.Children[1].X);
		}


		[Fact]
		public void LayoutShiftedOffCenterTallMode()
		{
			var testDualScreenService = new TestDualScreenServiceLandscape();
			testDualScreenService.SetLocationOnScreen(new Point(0, 400));
			var result = CreateTwoPaneView(testDualScreenService);

			result.Layout(new Rectangle(0, 400, testDualScreenService.DeviceInfo.ScaledScreenSize.Width, 500));

			Assert.Equal(90, result.Children[0].Height);
			Assert.Equal(400, result.Bounds.Y);
			Assert.Equal(390, result.Children[1].Height);
			Assert.Equal(110, result.Children[1].Y);
		}



		[Fact]
		public void PortraitLayoutStartUnderneathHinge()
		{
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait();
			testDualScreenService.SetLocationOnScreen(new Point(500, 0));
			var result = CreateTwoPaneView(dualScreenService: testDualScreenService);

			result.Layout(new Rectangle(500, 0, 400, testDualScreenService.DeviceInfo.ScaledScreenSize.Height));

			Assert.Equal(390, result.Pane1.Width);
			Assert.Equal(10, result.Pane1.X);
			Assert.False(result.Children[1].IsVisible);
		}

		[Fact]
		public void PortraitLayoutEndsUnderneathHinge()
		{
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait();
			testDualScreenService.SetLocationOnScreen(new Point(100, 0));
			var result = CreateTwoPaneView(dualScreenService: testDualScreenService);

			result.Layout(new Rectangle(100, 0, 400, testDualScreenService.DeviceInfo.ScaledScreenSize.Height));

			Assert.Equal(390, result.Pane1.Width);
			Assert.Equal(0, result.Pane1.X);
			Assert.False(result.Children[1].IsVisible);
		}


		[Fact]
		public void LandscapeLayoutStartUnderneathHinge()
		{
			var testDualScreenService = new TestDualScreenServiceLandscape();
			testDualScreenService.SetLocationOnScreen(new Point(0, 500));
			var result = CreateTwoPaneView(dualScreenService: testDualScreenService);
			result.Layout(new Rectangle(0, 500, testDualScreenService.DeviceInfo.ScaledScreenSize.Width, 400));

			Assert.Equal(390, result.Pane1.Height);
			Assert.Equal(10, result.Pane1.Y);
			Assert.False(result.Children[1].IsVisible);
		}

		[Fact]
		public void LandscapeLayoutEndsUnderneathHinge()
		{
			var testDualScreenService = new TestDualScreenServiceLandscape();
			testDualScreenService.SetLocationOnScreen(new Point(0, 100));
			var result = CreateTwoPaneView(dualScreenService: testDualScreenService);

			result.Layout(new Rectangle(0, 100, testDualScreenService.DeviceInfo.ScaledScreenSize.Width, 400));

			Assert.Equal(390, result.Pane1.Height);
			Assert.Equal(0, result.Pane1.Y);
			Assert.False(result.Children[1].IsVisible);
		}


		[Fact]
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

			Assert.Equal(twoPaneViewNested.Mode, TwoPaneViewMode.Wide);

			Assert.Equal(90, twoPaneViewNested.Pane1.Width);
			Assert.Equal(90, twoPaneViewNested.Pane2.Width);
			Assert.Equal(200, twoPaneViewNested.Pane1.Height);
			Assert.Equal(200, twoPaneViewNested.Pane2.Height);

			Assert.Equal(0, twoPaneViewNested.Pane1.X);
			Assert.Equal(0, twoPaneViewNested.Pane2.X);
		}

		[Fact]
		public void SpanningBoundsLandscape()
		{
			var testDualScreenService = new TestDualScreenServiceLandscape();
			testDualScreenService.IsSpanned = true;
			testDualScreenService.SetLocationOnScreen(new Point(0, 400));
			var result = new StackLayout() { IsPlatformEnabled = true };

			result.Layout(new Rectangle(0, 400, testDualScreenService.ScaledScreenSize.Width, 200));
			DualScreenInfo info = new DualScreenInfo(result, testDualScreenService);


			var top = info.SpanningBounds[0];

			Assert.Equal(0, top.X);
			Assert.Equal(0, top.Y);
			Assert.Equal(testDualScreenService.ScaledScreenSize.Width, top.Width);
			Assert.Equal(90, top.Height);

			var bottom = info.SpanningBounds[1];
			Assert.Equal(0, bottom.X);
			Assert.Equal(110, bottom.Y);
			Assert.Equal(testDualScreenService.ScaledScreenSize.Width, bottom.Width);
			Assert.Equal(90, bottom.Height);
		}

		[Fact]
		public void SpanningBoundsPortrait()
		{
			var testDualScreenService = new TestDualScreenServicePortrait();
			testDualScreenService.IsSpanned = true;
			testDualScreenService.SetLocationOnScreen(new Point(400, 0));
			var result = new StackLayout() { IsPlatformEnabled = true };

			result.Layout(new Rectangle(400, 0, 200, testDualScreenService.ScaledScreenSize.Height));
			DualScreenInfo info = new DualScreenInfo(result, testDualScreenService);


			var left = info.SpanningBounds[0];

			Assert.Equal(0, left.X);
			Assert.Equal(0, left.Y);
			Assert.Equal(90, left.Width);
			Assert.Equal(testDualScreenService.ScaledScreenSize.Height, left.Height);

			var right = info.SpanningBounds[1];
			Assert.Equal(110, right.X);
			Assert.Equal(0, right.Y);
			Assert.Equal(90, right.Width);
			Assert.Equal(testDualScreenService.ScaledScreenSize.Height, right.Height);
		}
	}
}