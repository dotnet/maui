using Microsoft.Maui;
using Microsoft.Maui.Layouts;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.UnitTests.Layouts
{
	[Category(TestCategory.Core, TestCategory.Layout)]
	public class LayoutExtensionTests
	{
		[Fact]
		public void FrameExcludesMargin()
		{
			var element = Substitute.For<IView>();
			var margin = new Thickness(20);
			element.Margin.Returns(margin);

			var bounds = new Rectangle(0, 0, 100, 100);
			var frame = element.ComputeFrame(bounds);

			// With a margin of 20 all the way around, we expect the actual frame
			// to be 60x60, with an x,y position of 20,20

			Assert.Equal(20, frame.Top);
			Assert.Equal(20, frame.Left);
			Assert.Equal(60, frame.Width);
			Assert.Equal(60, frame.Height);
		}

		[Fact]
		public void FrameSizeGoesToZeroWhenMarginsExceedBounds()
		{
			var element = Substitute.For<IView>();
			var margin = new Thickness(200);
			element.Margin.Returns(margin);

			var bounds = new Rectangle(0, 0, 100, 100);
			var frame = element.ComputeFrame(bounds);

			// The margin is simply too large for the bounds; since negative widths/heights on a frame don't make sense,
			// we expect them to collapse to zero

			Assert.Equal(0, frame.Height);
			Assert.Equal(0, frame.Width);
		}

		[Fact]
		public void DesiredSizeIncludesMargin()
		{
			var widthConstraint = 400;
			var heightConstraint = 655;

			var handler = Substitute.For<IViewHandler>();
			var element = Substitute.For<IView>();
			var margin = new Thickness(20);

			// Our "native" control will request a size of (100,50) when we call GetDesiredSize
			handler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(new Size(100, 50));
			element.Handler.Returns(handler);
			element.Margin.Returns(margin);

			var desiredSize = element.ComputeDesiredSize(widthConstraint, heightConstraint);

			// Because the actual ("native") measurement comes back with (100,50)
			// and the margin on the IFrameworkElement is 20, the expected width is (100 + 20 + 20) = 140
			// and the expected height is (50 + 20 + 20) = 90

			Assert.Equal(140, desiredSize.Width);
			Assert.Equal(90, desiredSize.Height);
		}
	}
}
