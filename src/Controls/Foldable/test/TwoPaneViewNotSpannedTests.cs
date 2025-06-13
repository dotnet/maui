using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Foldable.UnitTests
{
	// [TestFixture] - removed for xUnit
	public partial class TwoPaneViewNotSpannedTests : BaseTestFixture
	{
		TwoPaneView CreateTwoPaneView(View pane1 = null, View pane2 = null, IDualScreenService dualScreenService = null)
		{
			dualScreenService = dualScreenService ?? new TestDualScreenServicePortrait();
			pane1 = pane1 ?? new BoxView();
			pane2 = pane2 ?? new BoxView();

			TwoPaneView view = new TwoPaneView(dualScreenService)
			{
				IsPlatformEnabled = true,
				Pane1 = pane1,
				Pane2 = pane2
			};

			if (pane1 != null)
				pane1.IsPlatformEnabled = true;

			if (pane2 != null)
				pane2.IsPlatformEnabled = true;

			view.Children[0].IsPlatformEnabled = true;
			view.Children[1].IsPlatformEnabled = true;

			return view;
		}


		[Fact]
		public void DeviceWithoutSpanSupport()
		{
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait();
			var result = CreateTwoPaneView(dualScreenService: testDualScreenService);

			result.Layout(new Rectangle(100, 100, 200, 200));

			Assert.Equal(200, result.Children[0].Width);
			Assert.Equal(200, result.Children[0].Height);
		}



		[Fact]
		public void GettersAndSetters()
		{
			var Pane1 = new StackLayout();
			var Pane2 = new Grid();

			TwoPaneView twoPaneView = CreateTwoPaneView(Pane1, Pane2);

			twoPaneView.TallModeConfiguration = TwoPaneViewTallModeConfiguration.SinglePane;
			twoPaneView.WideModeConfiguration = TwoPaneViewWideModeConfiguration.SinglePane;
			twoPaneView.Pane1 = Pane1;
			twoPaneView.Pane2 = Pane2;
			twoPaneView.PanePriority = TwoPaneViewPriority.Pane2;
			twoPaneView.MinTallModeHeight = 1000;
			twoPaneView.MinWideModeWidth = 2000;


			Assert.Equal(TwoPaneViewTallModeConfiguration.SinglePane, twoPaneView.TallModeConfiguration);
			Assert.Equal(TwoPaneViewWideModeConfiguration.SinglePane, twoPaneView.WideModeConfiguration);
			Assert.Equal(Pane1, twoPaneView.Pane1);
			Assert.Equal(Pane2, twoPaneView.Pane2);
			Assert.Equal(TwoPaneViewPriority.Pane2, twoPaneView.PanePriority);
			Assert.Equal(1000, twoPaneView.MinTallModeHeight);
			Assert.Equal(2000, twoPaneView.MinWideModeWidth);
		}

		[Fact]
		public void BasicLayoutTestPortrait()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView();
			twoPaneView.Layout(new Rectangle(0, 0, 300, 500));

			Assert.Equal(300, twoPaneView.Pane1.Width);
			Assert.Equal(500, twoPaneView.Pane1.Height);

		}

		[Fact]
		public void BasicLayoutTestLandscape()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView(dualScreenService: new TestDualScreenServiceLandscape());
			twoPaneView.Layout(new Rectangle(0, 0, 300, 500));

			Assert.Equal(300, twoPaneView.Width);
			Assert.Equal(500, twoPaneView.Height);
		}

		[Fact]
		public void BasicLayoutTestSinglePanePortrait()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView();
			twoPaneView.MinWideModeWidth = 1000;
			twoPaneView.MinTallModeHeight = 1000;
			twoPaneView.Layout(new Rectangle(0, 0, 300, 500));

			Assert.Equal(300, twoPaneView.Pane1.Width);
			Assert.Equal(500, twoPaneView.Pane1.Height);

		}

		[Fact]
		public void BasicLayoutTestSinglePaneLandscape()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView(dualScreenService: new TestDualScreenServiceLandscape());
			twoPaneView.MinWideModeWidth = 1000;
			twoPaneView.MinTallModeHeight = 1000;
			twoPaneView.Layout(new Rectangle(0, 0, 300, 500));

			Assert.Equal(300, twoPaneView.Pane1.Width);
			Assert.Equal(500, twoPaneView.Pane1.Height);
		}

		[Fact]
		public void ModeSwitchesWithMinWideModeWidth()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView();
			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));

			twoPaneView.MinWideModeWidth = 400;
			Assert.Equal(TwoPaneViewMode.SinglePane, twoPaneView.Mode);
			twoPaneView.MinWideModeWidth = 100;
			Assert.Equal(TwoPaneViewMode.Wide, twoPaneView.Mode);
		}

		[Fact]
		public void ModeSwitchesWithMinTallModeHeight()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView();
			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));

			twoPaneView.MinTallModeHeight = 400;
			Assert.Equal(TwoPaneViewMode.SinglePane, twoPaneView.Mode);
			twoPaneView.MinTallModeHeight = 100;
			Assert.Equal(TwoPaneViewMode.Tall, twoPaneView.Mode);
		}

		[Fact]
		public void Pane1LengthTallMode()
		{
			var pane1 = new BoxView();
			var pane2 = new BoxView();

			TwoPaneView twoPaneView = CreateTwoPaneView(pane1, pane2);

			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));
			twoPaneView.MinTallModeHeight = 100;
			Assert.AreNotEqual(100, twoPaneView.Pane1.Height);
			twoPaneView.Pane1Length = 100;
			Assert.Equal(100, twoPaneView.Pane1.Height);
		}


		[Fact]
		public void Pane1LengthWideMode()
		{
			var pane1 = new BoxView();
			var pane2 = new BoxView();

			TwoPaneView twoPaneView = CreateTwoPaneView(pane1, pane2);

			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));
			twoPaneView.MinWideModeWidth = 100;
			Assert.AreNotEqual(100, twoPaneView.Pane1.Width);
			twoPaneView.Pane1Length = 100;
			Assert.Equal(100, twoPaneView.Pane1.Width);
		}


		[Fact]
		public void Pane2LengthTallMode()
		{
			var pane1 = new BoxView();
			var pane2 = new BoxView();

			TwoPaneView twoPaneView = CreateTwoPaneView(pane1, pane2);

			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));
			twoPaneView.MinTallModeHeight = 100;
			Assert.AreNotEqual(100, twoPaneView.Pane2.Height);
			twoPaneView.Pane2Length = 100;
			Assert.Equal(100, twoPaneView.Pane2.Height);
		}


		[Fact]
		public void Pane2LengthWideMode()
		{
			var pane1 = new BoxView();
			var pane2 = new BoxView();

			TwoPaneView twoPaneView = CreateTwoPaneView(pane1, pane2);

			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));
			twoPaneView.MinWideModeWidth = 100;
			Assert.AreNotEqual(100, twoPaneView.Pane2.Width);
			twoPaneView.Pane2Length = 100;
			Assert.Equal(100, twoPaneView.Pane2.Width);
		}


		[Fact]
		public void PanePriority()
		{
			var pane1 = new BoxView();
			var pane2 = new BoxView();

			TwoPaneView twoPaneView = CreateTwoPaneView(pane1, pane2);

			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));
			twoPaneView.MinWideModeWidth = 500;
			twoPaneView.MinTallModeHeight = 500;

			Assert.False(twoPaneView.Children[1].IsVisible);
			Assert.True(twoPaneView.Children[0].IsVisible);

			Assert.Equal(pane1.Height, 300);
			Assert.Equal(pane1.Width, 300);

			twoPaneView.PanePriority = TwoPaneViewPriority.Pane2;

			Assert.Equal(pane2.Height, 300);
			Assert.Equal(pane2.Width, 300);

			Assert.False(twoPaneView.Children[0].IsVisible);
			Assert.True(twoPaneView.Children[1].IsVisible);
		}

		[Fact]
		public void TallModeConfiguration()
		{
			var pane1 = new BoxView();
			var pane2 = new BoxView();
			TwoPaneView twoPaneView = CreateTwoPaneView(pane1, pane2);
			twoPaneView.MinTallModeHeight = 100;
			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));

			Assert.Equal(0, twoPaneView.Children[0].Bounds.Y);
			Assert.Equal(150, twoPaneView.Children[1].Bounds.Y);

			twoPaneView.TallModeConfiguration = TwoPaneViewTallModeConfiguration.BottomTop;

			Assert.Equal(150, twoPaneView.Children[0].Bounds.Y);
			Assert.Equal(0, twoPaneView.Children[1].Bounds.Y);

			twoPaneView.TallModeConfiguration = TwoPaneViewTallModeConfiguration.SinglePane;

			Assert.True(twoPaneView.Children[0].IsVisible);
			Assert.False(twoPaneView.Children[1].IsVisible);

			twoPaneView.PanePriority = TwoPaneViewPriority.Pane2;

			Assert.False(twoPaneView.Children[0].IsVisible);
			Assert.True(twoPaneView.Children[1].IsVisible);

			twoPaneView.PanePriority = TwoPaneViewPriority.Pane1;

			Assert.True(twoPaneView.Children[0].IsVisible);
			Assert.False(twoPaneView.Children[1].IsVisible);
		}

		[Fact]
		public void WideModeConfiguration()
		{
			var pane1 = new BoxView();
			var pane2 = new BoxView();
			TwoPaneView twoPaneView = CreateTwoPaneView(pane1, pane2);
			twoPaneView.MinWideModeWidth = 100;
			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));

			Assert.Equal(0, twoPaneView.Children[0].Bounds.X);
			Assert.Equal(150, twoPaneView.Children[1].Bounds.X);

			twoPaneView.WideModeConfiguration = TwoPaneViewWideModeConfiguration.RightLeft;

			Assert.Equal(150, twoPaneView.Children[0].Bounds.X);
			Assert.Equal(0, twoPaneView.Children[1].Bounds.X);

			twoPaneView.WideModeConfiguration = TwoPaneViewWideModeConfiguration.SinglePane;

			Assert.True(twoPaneView.Children[0].IsVisible);
			Assert.False(twoPaneView.Children[1].IsVisible);

			twoPaneView.PanePriority = TwoPaneViewPriority.Pane2;

			Assert.False(twoPaneView.Children[0].IsVisible);
			Assert.True(twoPaneView.Children[1].IsVisible);

			twoPaneView.PanePriority = TwoPaneViewPriority.Pane1;

			Assert.True(twoPaneView.Children[0].IsVisible);
			Assert.False(twoPaneView.Children[1].IsVisible);
		}

		[Fact]
		public void Pane1BecomesVisibleAfterOnlyPane2IsVisibleWideMode()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView();
			twoPaneView.Layout(new Rectangle(0, 0, 300, 500));
			twoPaneView.MinWideModeWidth = 4000;
			twoPaneView.PanePriority = TwoPaneViewPriority.Pane2;

			Assert.False(twoPaneView.Children[0].IsVisible);
			Assert.True(twoPaneView.Children[1].IsVisible);
			twoPaneView.MinWideModeWidth = 0;
			Assert.Equal(twoPaneView.Mode, TwoPaneViewMode.Wide);
			Assert.True(twoPaneView.Children[0].IsVisible);
			Assert.True(twoPaneView.Children[1].IsVisible);
		}

		[Fact]
		public void Pane2BecomesVisibleAfterOnlyPane1IsVisibleWideMode()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView();
			twoPaneView.Layout(new Rectangle(0, 0, 300, 500));
			twoPaneView.MinWideModeWidth = 4000;
			twoPaneView.PanePriority = TwoPaneViewPriority.Pane1;

			Assert.True(twoPaneView.Children[0].IsVisible);
			Assert.False(twoPaneView.Children[1].IsVisible);
			twoPaneView.MinWideModeWidth = 0;
			Assert.Equal(twoPaneView.Mode, TwoPaneViewMode.Wide);
			Assert.True(twoPaneView.Children[0].IsVisible);
			Assert.True(twoPaneView.Children[1].IsVisible);
		}

		[Fact]
		public void Pane1BecomesVisibleAfterOnlyPane2IsVisibleTallMode()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView();
			twoPaneView.Layout(new Rectangle(0, 0, 300, 500));
			twoPaneView.MinTallModeHeight = 4000;
			twoPaneView.PanePriority = TwoPaneViewPriority.Pane2;

			Assert.False(twoPaneView.Children[0].IsVisible);
			Assert.True(twoPaneView.Children[1].IsVisible);
			twoPaneView.MinTallModeHeight = 0;
			Assert.Equal(twoPaneView.Mode, TwoPaneViewMode.Tall);
			Assert.True(twoPaneView.Children[0].IsVisible);
			Assert.True(twoPaneView.Children[1].IsVisible);
		}

		[Fact]
		public void Pane2BecomesVisibleAfterOnlyPane1IsVisibleTallMode()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView();
			twoPaneView.Layout(new Rectangle(0, 0, 300, 500));
			twoPaneView.MinTallModeHeight = 4000;
			twoPaneView.PanePriority = TwoPaneViewPriority.Pane1;

			Assert.True(twoPaneView.Children[0].IsVisible);
			Assert.False(twoPaneView.Children[1].IsVisible);
			twoPaneView.MinTallModeHeight = 0;
			Assert.Equal(twoPaneView.Mode, TwoPaneViewMode.Tall);
			Assert.True(twoPaneView.Children[0].IsVisible);
			Assert.True(twoPaneView.Children[1].IsVisible);
		}
	}
}
