using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class MarginTests : BaseTestFixture
	{
		[Fact]
		public void GetSizeRequestIncludesMargins()
		{
			var parent = new ContentView
			{
				IsPlatformEnabled = true,
			};

			var mockHandler = Substitute.For<IViewHandler>();
			mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo => new Size(100, 50));
			var child = MockPlatformSizeService.Sub<Button>(GetPlatformSize, text: "Test");
			child.Handler = mockHandler;

			child.Margin = new Thickness(10, 20, 30, 40);
			parent.Content = child;

			var result = (parent as ICrossPlatformLayout).CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
			Assert.Equal(new Size(140, 110), result);
		}

		[Fact]
		public void MarginsAffectPositionInContentView()
		{
			var parent = new ContentView
			{
				IsPlatformEnabled = true,
			};

			var mockHandler = Substitute.For<IViewHandler>();
			mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo => new Size(100, 50));
			var child = MockPlatformSizeService.Sub<Button>(GetPlatformSize, text: "Test");
			child.Handler = mockHandler;

			child.Margin = new Thickness(10, 20, 30, 40);
			parent.Content = child;

			var crossPlatformLayout = (ICrossPlatformLayout)parent;
			crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 140, 110));
			Assert.Equal(new Rect(10, 20, 100, 50), child.Bounds);
		}

		[Fact]
		public void ChangingMarginCausesRelayout()
		{
			var parent = new ContentView
			{
				IsPlatformEnabled = true,
			};

			var mockHandler = Substitute.For<IViewHandler>();
			mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo => new Size(100, 50));
			var child = MockPlatformSizeService.Sub<Button>(
				GetPlatformSize,
				text: "Test",
				horizOpts: LayoutOptions.Start,
				vertOpts: LayoutOptions.Start);
			child.Handler = mockHandler;

			child.Margin = new Thickness(10, 20, 30, 40);
			parent.Content = child;

			var crossPlatformLayout = (ICrossPlatformLayout)parent;
			crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 1000, 1000));
			Assert.Equal(new Rect(10, 20, 100, 50), child.Bounds);
		}

		void AssertInvalidated(IViewHandler handler)
		{
			handler.Received().Invoke(Arg.Is(nameof(IView.InvalidateMeasure)), Arg.Any<object>());
			handler.ClearReceivedCalls();
		}

		static SizeRequest GetPlatformSize(VisualElement _, double w, double h) => new(new(100, 50));
	}
}