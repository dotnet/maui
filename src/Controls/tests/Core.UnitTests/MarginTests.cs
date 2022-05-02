using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;

	public class MarginTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			MockPlatformSizeService.Current.GetPlatformSizeFunc = (b, d, e) => new SizeRequest(new Size(100, 50));
		}

		[Test]
		public void GetSizeRequestIncludesMargins()
		{
			var parent = new ContentView
			{
				IsPlatformEnabled = true,
			};
			var child = new Button
			{
				Text = "Test",
				IsPlatformEnabled = true,
			};


			child.Margin = new Thickness(10, 20, 30, 40);
			parent.Content = child;

			var result = parent.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
			Assert.AreEqual(new Size(140, 110), result.Request);
		}

		[Test]
		public void MarginsAffectPositionInContentView()
		{
			var parent = new ContentView
			{
				IsPlatformEnabled = true,
			};
			var child = new Button
			{
				Text = "Test",
				IsPlatformEnabled = true,
			};


			child.Margin = new Thickness(10, 20, 30, 40);
			parent.Content = child;

			parent.Layout(new Rect(0, 0, 140, 110));
			Assert.AreEqual(new Rect(10, 20, 100, 50), child.Bounds);
		}

		[Test]
		public void ChangingMarginCausesRelayout()
		{
			var parent = new ContentView
			{
				IsPlatformEnabled = true,
			};
			var child = new Button
			{
				Text = "Test",
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Start,
				IsPlatformEnabled = true,
			};


			child.Margin = new Thickness(10, 20, 30, 40);
			parent.Content = child;

			parent.Layout(new Rect(0, 0, 1000, 1000));
			Assert.AreEqual(new Rect(10, 20, 100, 50), child.Bounds);
		}

		[Test]
		public void IntegrationTest()
		{
			var parent = new StackLayout
			{
				Spacing = 0,
				IsPlatformEnabled = true,
			};

			var handler = Substitute.For<IViewHandler>();
			parent.Handler = handler;

			var child1 = new Button
			{
				Text = "Test",
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Start,
				IsPlatformEnabled = true,
			};

			var child2 = new Button
			{
				Text = "Test",
				IsPlatformEnabled = true,
			};

			child2.Margin = new Thickness(5, 10, 15, 20);

			parent.Children.Add(child1);
			parent.Children.Add(child2);

			parent.Layout(new Rect(0, 0, 1000, 1000));

			Assert.AreEqual(new Rect(0, 0, 100, 50), child1.Bounds);
			Assert.AreEqual(new Rect(5, 60, 980, 50), child2.Bounds);

			child1.Margin = new Thickness(10, 20, 30, 40);

			// Verify that the margin change invalidated the layout, and simulate a native layout change
			AssertInvalidated(handler);
			parent.ForceLayout();

			Assert.AreEqual(new Rect(10, 20, 100, 50), child1.Bounds);
			Assert.AreEqual(new Rect(5, 120, 980, 50), child2.Bounds);
		}

		void AssertInvalidated(IViewHandler handler)
		{
			handler.Received().Invoke(Arg.Is(nameof(IView.InvalidateMeasure)), Arg.Any<object>());
			handler.ClearReceivedCalls();
		}
	}
}