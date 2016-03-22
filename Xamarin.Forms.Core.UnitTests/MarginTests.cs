using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Core.UnitTests
{
	public class MarginTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup ();
			Device.PlatformServices = new MockPlatformServices ();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown ();
			Device.PlatformServices = null;
		}

		[Test]
		public void GetSizeRequestIncludesMargins ()
		{
			var platform = new UnitPlatform ((b, d, e) => new SizeRequest(new Size(100,50)));

			var parent = new ContentView {
				Platform = platform,
				IsPlatformEnabled = true,
			};
			var child = new Button {
				Text = "Test",
				Platform = platform,
				IsPlatformEnabled = true,
			};
			

			child.Margin = new Thickness (10, 20, 30, 40);
			parent.Content = child;

			var result = parent.Measure (double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
			Assert.AreEqual (new Size (140, 110), result.Request);
		}

		[Test]
		public void MarginsAffectPositionInContentView ()
		{
			var platform = new UnitPlatform ((b, d, e) => new SizeRequest(new Size(100,50)));

			var parent = new ContentView {
				Platform = platform,
				IsPlatformEnabled = true,
			};
			var child = new Button {
				Text = "Test",
				Platform = platform,
				IsPlatformEnabled = true,
			};
			

			child.Margin = new Thickness (10, 20, 30, 40);
			parent.Content = child;

			parent.Layout (new Rectangle (0, 0, 140, 110));
			Assert.AreEqual (new Rectangle (10, 20, 100, 50), child.Bounds);
		}

		[Test]
		public void ChangingMarginCausesRelayout ()
		{
			var platform = new UnitPlatform ((b, d, e) => new SizeRequest(new Size(100,50)));

			var parent = new ContentView {
				Platform = platform,
				IsPlatformEnabled = true,
			};
			var child = new Button {
				Text = "Test",
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Start,
				Platform = platform,
				IsPlatformEnabled = true,
			};
			

			child.Margin = new Thickness (10, 20, 30, 40);
			parent.Content = child;

			parent.Layout (new Rectangle (0, 0, 1000, 1000));
			Assert.AreEqual (new Rectangle (10, 20, 100, 50), child.Bounds);
		}

		[Test]
		public void IntegrationTest ()
		{
			var platform = new UnitPlatform ((b, d, e) => new SizeRequest(new Size(100,50)));

			var parent = new StackLayout {
				Platform = platform,
				Spacing = 0,
				IsPlatformEnabled = true,
			};

			var child1 = new Button {
				Text = "Test",
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Start,
				Platform = platform,
				IsPlatformEnabled = true,
			};

			var child2 = new Button {
				Text = "Test",
				Platform = platform,
				IsPlatformEnabled = true,
			};

			child2.Margin = new Thickness (5, 10, 15, 20);

			parent.Children.Add (child1);
			parent.Children.Add (child2);

			parent.Layout (new Rectangle (0, 0, 1000, 1000));

			Assert.AreEqual (new Rectangle (0, 0, 100, 50), child1.Bounds);
			Assert.AreEqual (new Rectangle (5, 60, 980, 50), child2.Bounds);

			child1.Margin = new Thickness (10, 20, 30, 40);

			Assert.AreEqual (new Rectangle (10, 20, 100, 50), child1.Bounds);
			Assert.AreEqual (new Rectangle (5, 120, 980, 50), child2.Bounds);
		}
	}
}