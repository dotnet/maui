using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.DualScreen.UnitTests
{
	[TestFixture]
	public partial class TwoPaneViewNotSpannedTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

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


		[Test]
		public void DeviceWithoutSpanSupport()
		{
			TestDualScreenServicePortrait testDualScreenService = new TestDualScreenServicePortrait();
			var result = CreateTwoPaneView(dualScreenService: testDualScreenService);

			result.Layout(new Rectangle(100, 100, 200, 200));

			Assert.AreEqual(200, result.Children[0].Width);
			Assert.AreEqual(200, result.Children[0].Height);
		}



		[Test]
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


			Assert.AreEqual(TwoPaneViewTallModeConfiguration.SinglePane, twoPaneView.TallModeConfiguration);
			Assert.AreEqual(TwoPaneViewWideModeConfiguration.SinglePane, twoPaneView.WideModeConfiguration);
			Assert.AreEqual(Pane1, twoPaneView.Pane1);
			Assert.AreEqual(Pane2, twoPaneView.Pane2);
			Assert.AreEqual(TwoPaneViewPriority.Pane2, twoPaneView.PanePriority);
			Assert.AreEqual(1000, twoPaneView.MinTallModeHeight);
			Assert.AreEqual(2000, twoPaneView.MinWideModeWidth);
		}

		[Test]
		public void BasicLayoutTestPortrait()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView();
			twoPaneView.Layout(new Rectangle(0, 0, 300, 500));

			Assert.AreEqual(300, twoPaneView.Pane1.Width);
			Assert.AreEqual(500, twoPaneView.Pane1.Height);

		}

		[Test]
		public void BasicLayoutTestLandscape()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView(dualScreenService: new TestDualScreenServiceLandscape());
			twoPaneView.Layout(new Rectangle(0, 0, 300, 500));

			Assert.AreEqual(300, twoPaneView.Width);
			Assert.AreEqual(500, twoPaneView.Height);
		}

		[Test]
		public void BasicLayoutTestSinglePanePortrait()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView();
			twoPaneView.MinWideModeWidth = 1000;
			twoPaneView.MinTallModeHeight = 1000;
			twoPaneView.Layout(new Rectangle(0, 0, 300, 500));

			Assert.AreEqual(300, twoPaneView.Pane1.Width);
			Assert.AreEqual(500, twoPaneView.Pane1.Height);

		}

		[Test]
		public void BasicLayoutTestSinglePaneLandscape()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView(dualScreenService: new TestDualScreenServiceLandscape());
			twoPaneView.MinWideModeWidth = 1000;
			twoPaneView.MinTallModeHeight = 1000;
			twoPaneView.Layout(new Rectangle(0, 0, 300, 500));

			Assert.AreEqual(300, twoPaneView.Pane1.Width);
			Assert.AreEqual(500, twoPaneView.Pane1.Height);
		}

		[Test]
		public void ModeSwitchesWithMinWideModeWidth()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView();
			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));

			twoPaneView.MinWideModeWidth = 400;
			Assert.AreEqual(TwoPaneViewMode.SinglePane, twoPaneView.Mode);
			twoPaneView.MinWideModeWidth = 100;
			Assert.AreEqual(TwoPaneViewMode.Wide, twoPaneView.Mode);
		}

		[Test]
		public void ModeSwitchesWithMinTallModeHeight()
		{
			TwoPaneView twoPaneView = CreateTwoPaneView();
			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));

			twoPaneView.MinTallModeHeight = 400;
			Assert.AreEqual(TwoPaneViewMode.SinglePane, twoPaneView.Mode);
			twoPaneView.MinTallModeHeight = 100;
			Assert.AreEqual(TwoPaneViewMode.Tall, twoPaneView.Mode);
		}

		[Test]
		public void Pane1LengthTallMode()
		{
			var pane1 = new BoxView();
			var pane2 = new BoxView();

			TwoPaneView twoPaneView = CreateTwoPaneView(pane1, pane2);

			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));
			twoPaneView.MinTallModeHeight = 100;
			Assert.AreNotEqual(100, twoPaneView.Pane1.Height);
			twoPaneView.Pane1Length = 100;
			Assert.AreEqual(100, twoPaneView.Pane1.Height);
		}


		[Test]
		public void Pane1LengthWideMode()
		{
			var pane1 = new BoxView();
			var pane2 = new BoxView();

			TwoPaneView twoPaneView = CreateTwoPaneView(pane1, pane2);

			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));
			twoPaneView.MinWideModeWidth = 100;
			Assert.AreNotEqual(100, twoPaneView.Pane1.Width);
			twoPaneView.Pane1Length = 100;
			Assert.AreEqual(100, twoPaneView.Pane1.Width);
		}


		[Test]
		public void Pane2LengthTallMode()
		{
			var pane1 = new BoxView();
			var pane2 = new BoxView();

			TwoPaneView twoPaneView = CreateTwoPaneView(pane1, pane2);

			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));
			twoPaneView.MinTallModeHeight = 100;
			Assert.AreNotEqual(100, twoPaneView.Pane2.Height);
			twoPaneView.Pane2Length = 100;
			Assert.AreEqual(100, twoPaneView.Pane2.Height);
		}


		[Test]
		public void Pane2LengthWideMode()
		{
			var pane1 = new BoxView();
			var pane2 = new BoxView();

			TwoPaneView twoPaneView = CreateTwoPaneView(pane1, pane2);

			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));
			twoPaneView.MinWideModeWidth = 100;
			Assert.AreNotEqual(100, twoPaneView.Pane2.Width);
			twoPaneView.Pane2Length = 100;
			Assert.AreEqual(100, twoPaneView.Pane2.Width);
		}


		[Test]
		public void PanePriority()
		{
			var pane1 = new BoxView();
			var pane2 = new BoxView();

			TwoPaneView twoPaneView = CreateTwoPaneView(pane1, pane2);

			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));
			twoPaneView.MinWideModeWidth = 500;
			twoPaneView.MinTallModeHeight = 500;

			Assert.IsFalse(twoPaneView.Children[1].IsVisible);
			Assert.IsTrue(twoPaneView.Children[0].IsVisible);

			Assert.AreEqual(pane1.Height, 300);
			Assert.AreEqual(pane1.Width, 300);

			twoPaneView.PanePriority = TwoPaneViewPriority.Pane2;

			Assert.AreEqual(pane2.Height, 300);
			Assert.AreEqual(pane2.Width, 300);

			Assert.IsFalse(twoPaneView.Children[0].IsVisible);
			Assert.IsTrue(twoPaneView.Children[1].IsVisible);
		}

		[Test]
		public void TallModeConfiguration()
		{
			var pane1 = new BoxView();
			var pane2 = new BoxView();
			TwoPaneView twoPaneView = CreateTwoPaneView(pane1, pane2);
			twoPaneView.MinTallModeHeight = 100;
			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));

			Assert.AreEqual(0, twoPaneView.Children[0].Bounds.Y);
			Assert.AreEqual(150, twoPaneView.Children[1].Bounds.Y);

			twoPaneView.TallModeConfiguration = TwoPaneViewTallModeConfiguration.BottomTop;

			Assert.AreEqual(150, twoPaneView.Children[0].Bounds.Y);
			Assert.AreEqual(0, twoPaneView.Children[1].Bounds.Y);

			twoPaneView.TallModeConfiguration = TwoPaneViewTallModeConfiguration.SinglePane;

			Assert.IsTrue(twoPaneView.Children[0].IsVisible);
			Assert.IsFalse(twoPaneView.Children[1].IsVisible);

			twoPaneView.PanePriority = TwoPaneViewPriority.Pane2;

			Assert.IsFalse(twoPaneView.Children[0].IsVisible);
			Assert.IsTrue(twoPaneView.Children[1].IsVisible);

			twoPaneView.PanePriority = TwoPaneViewPriority.Pane1;

			Assert.IsTrue(twoPaneView.Children[0].IsVisible);
			Assert.IsFalse(twoPaneView.Children[1].IsVisible);
		}

		[Test]
		public void WideModeConfiguration()
		{
			var pane1 = new BoxView();
			var pane2 = new BoxView();
			TwoPaneView twoPaneView = CreateTwoPaneView(pane1, pane2);
			twoPaneView.MinWideModeWidth = 100;
			twoPaneView.Layout(new Rectangle(0, 0, 300, 300));

			Assert.AreEqual(0, twoPaneView.Children[0].Bounds.X);
			Assert.AreEqual(150, twoPaneView.Children[1].Bounds.X);

			twoPaneView.WideModeConfiguration = TwoPaneViewWideModeConfiguration.RightLeft;

			Assert.AreEqual(150, twoPaneView.Children[0].Bounds.X);
			Assert.AreEqual(0, twoPaneView.Children[1].Bounds.X);

			twoPaneView.WideModeConfiguration = TwoPaneViewWideModeConfiguration.SinglePane;

			Assert.IsTrue(twoPaneView.Children[0].IsVisible);
			Assert.IsFalse(twoPaneView.Children[1].IsVisible);

			twoPaneView.PanePriority = TwoPaneViewPriority.Pane2;

			Assert.IsFalse(twoPaneView.Children[0].IsVisible);
			Assert.IsTrue(twoPaneView.Children[1].IsVisible);

			twoPaneView.PanePriority = TwoPaneViewPriority.Pane1;

			Assert.IsTrue(twoPaneView.Children[0].IsVisible);
			Assert.IsFalse(twoPaneView.Children[1].IsVisible);
		}
	}
}
