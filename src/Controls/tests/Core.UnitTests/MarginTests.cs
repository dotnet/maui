using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;

	public class MarginTests : BaseTestFixture
	{

		public MarginTests()
		{

			MockPlatformSizeService.Current.GetPlatformSizeFunc = (b, d, e) => new SizeRequest(new Size(100, 50));
		}

		[Fact]
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
			Assert.Equal(new Size(140, 110), result.Request);
		}

		[Fact]
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
			Assert.Equal(new Rect(10, 20, 100, 50), child.Bounds);
		}

		[Fact]
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
			Assert.Equal(new Rect(10, 20, 100, 50), child.Bounds);
		}

		[Fact]
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

			Assert.Equal(new Rect(0, 0, 100, 50), child1.Bounds);
			Assert.Equal(new Rect(5, 60, 980, 50), child2.Bounds);

			child1.Margin = new Thickness(10, 20, 30, 40);

			// Verify that the margin change invalidated the layout, and simulate a native layout change
			AssertInvalidated(handler);
			parent.ForceLayout();

			Assert.Equal(new Rect(10, 20, 100, 50), child1.Bounds);
			Assert.Equal(new Rect(5, 120, 980, 50), child2.Bounds);
		}

		void AssertInvalidated(IViewHandler handler)
		{
			handler.Received().Invoke(Arg.Is(nameof(IView.InvalidateMeasure)), Arg.Any<object>());
			handler.ClearReceivedCalls();
		}
	}
}