using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;
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
			element.Width.Returns(-1);
			element.Height.Returns(-1);

			var bounds = new Rectangle(0, 0, 100, 100);
			var frame = element.ComputeFrame(bounds);

			// With a margin of 20 all the way around, we expect the actual frame
			// to be 60x60, with an x,y position of 20,20

			Assert.Equal(20, frame.Top);
			Assert.Equal(20, frame.Left);
			Assert.Equal(60, frame.Width);
			Assert.Equal(60, frame.Height);
		}

		[Theory]
		[InlineData(LayoutAlignment.Fill)]
		[InlineData(LayoutAlignment.Start)]
		[InlineData(LayoutAlignment.Center)]
		[InlineData(LayoutAlignment.End)]
		public void FrameSizeGoesToZeroWhenMarginsExceedBounds(LayoutAlignment layoutAlignment)
		{
			var element = Substitute.For<IView>();
			var margin = new Thickness(200);
			element.Margin.Returns(margin);
			element.HorizontalLayoutAlignment.Returns(layoutAlignment);
			element.VerticalLayoutAlignment.Returns(layoutAlignment);

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

		public static IEnumerable<object[]> AlignmentTestData()
		{
			var margin = Thickness.Zero;

			// No margin
			yield return new object[] { LayoutAlignment.Start, margin, 0, 100 };
			yield return new object[] { LayoutAlignment.Center, margin, 100, 100 };
			yield return new object[] { LayoutAlignment.End, margin, 200, 100 };
			yield return new object[] { LayoutAlignment.Fill, margin, 0, 300 };

			// Even margin
			margin = new Thickness(10);
			yield return new object[] { LayoutAlignment.Start, margin, 10, 100 };
			yield return new object[] { LayoutAlignment.Center, margin, 100, 100 };
			yield return new object[] { LayoutAlignment.End, margin, 190, 100 };
			yield return new object[] { LayoutAlignment.Fill, margin, 10, 280 };

			//// Lopsided margin
			margin = new Thickness(5, 5, 10, 10);
			yield return new object[] { LayoutAlignment.Start, margin, 5, 100 };
			yield return new object[] { LayoutAlignment.Center, margin, 97.5, 100 };
			yield return new object[] { LayoutAlignment.End, margin, 190, 100 };
			yield return new object[] { LayoutAlignment.Fill, margin, 5, 285 };
		}

		[Theory]
		[MemberData(nameof(AlignmentTestData))]
		public void FrameAccountsForHorizontalLayoutAlignment(LayoutAlignment layoutAlignment, Thickness margin,
			double expectedX, double expectedWidth)
		{
			var widthConstraint = 300;
			var heightConstraint = 50;
			var viewSize = new Size(100, 50);

			var element = Substitute.For<IView>();

			element.Margin.Returns(margin);
			element.DesiredSize.Returns(viewSize);
			element.HorizontalLayoutAlignment.Returns(layoutAlignment);
			element.Width.Returns(-1);
			element.Height.Returns(-1);

			var frame = element.ComputeFrame(new Rectangle(0, 0, widthConstraint, heightConstraint));

			Assert.Equal(expectedX, frame.Left);
			Assert.Equal(expectedWidth, frame.Width);
		}

		[Theory]
		[MemberData(nameof(AlignmentTestData))]
		public void FrameAccountsForVerticalLayoutAlignment(LayoutAlignment layoutAlignment, Thickness margin,
			double expectedY, double expectedHeight)
		{
			var widthConstraint = 50;
			var heightConstraint = 300;
			var viewSize = new Size(50, 100);

			var element = Substitute.For<IView>();

			element.Margin.Returns(margin);
			element.DesiredSize.Returns(viewSize);
			element.VerticalLayoutAlignment.Returns(layoutAlignment);
			element.Width.Returns(-1);
			element.Height.Returns(-1);

			var frame = element.ComputeFrame(new Rectangle(0, 0, widthConstraint, heightConstraint));

			Assert.Equal(expectedY, frame.Top);
			Assert.Equal(expectedHeight, frame.Height);
		}

		public static IEnumerable<object[]> AlignmentTestDataRtl()
		{
			var margin = Thickness.Zero;

			// No margin
			yield return new object[] { LayoutAlignment.Start, margin, 200, 100 };
			yield return new object[] { LayoutAlignment.Center, margin, 100, 100 };
			yield return new object[] { LayoutAlignment.End, margin, 0, 100 };
			yield return new object[] { LayoutAlignment.Fill, margin, 0, 300 };

			// Even margin
			margin = new Thickness(10);
			yield return new object[] { LayoutAlignment.Start, margin, 190, 100 };
			yield return new object[] { LayoutAlignment.Center, margin, 100, 100 };
			yield return new object[] { LayoutAlignment.End, margin, 10, 100 };
			yield return new object[] { LayoutAlignment.Fill, margin, 10, 280 };

			// Lopsided margin
			margin = new Thickness(5, 5, 10, 10);
			yield return new object[] { LayoutAlignment.Start, margin, 195, 100 };
			yield return new object[] { LayoutAlignment.Center, margin, 102.5, 100 };
			yield return new object[] { LayoutAlignment.End, margin, 10, 100 };
			yield return new object[] { LayoutAlignment.Fill, margin, 10, 285 };
		}

		[Theory]
		[MemberData(nameof(AlignmentTestDataRtl))]
		public void FrameAccountsForHorizontalLayoutAlignmentRtl(LayoutAlignment layoutAlignment, Thickness margin,
			double expectedX, double expectedWidth)
		{
			var widthConstraint = 300;
			var heightConstraint = 50;
			var viewSize = new Size(100, 50);

			var element = Substitute.For<IView>();

			element.Margin.Returns(margin);
			element.DesiredSize.Returns(viewSize);
			element.FlowDirection.Returns(FlowDirection.RightToLeft);
			element.HorizontalLayoutAlignment.Returns(layoutAlignment);
			element.Width.Returns(-1);
			element.Height.Returns(-1);

			var frame = element.ComputeFrame(new Rectangle(0, 0, widthConstraint, heightConstraint));

			Assert.Equal(expectedX, frame.Left);
			Assert.Equal(expectedWidth, frame.Width);
		}
	}
}
