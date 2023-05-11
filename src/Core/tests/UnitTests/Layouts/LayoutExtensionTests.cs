using System;
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
			element.Width.Returns(Dimension.Unset);
			element.Height.Returns(Dimension.Unset);
			element.MaximumWidth.Returns(Dimension.Maximum);
			element.MaximumHeight.Returns(Dimension.Maximum);

			var bounds = new Rect(0, 0, 100, 100);
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

			var bounds = new Rect(0, 0, 100, 100);
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
			// and the margin on the IView is 20, the expected width is (100 + 20 + 20) = 140
			// and the expected height is (50 + 20 + 20) = 90

			Assert.Equal(140, desiredSize.Width);
			Assert.Equal(90, desiredSize.Height);
		}

		public static IEnumerable<object[]> AlignmentTestData()
		{
			var margin = Thickness.Zero;
			var point = Point.Zero;

			// No margin
			yield return new object[] { LayoutAlignment.Start, point, margin, 0, 100 };
			yield return new object[] { LayoutAlignment.Center, point, margin, 100, 100 };
			yield return new object[] { LayoutAlignment.End, point, margin, 200, 100 };
			yield return new object[] { LayoutAlignment.Fill, point, margin, 0, 300 };

			// Even margin
			margin = new Thickness(10);
			yield return new object[] { LayoutAlignment.Start, point, margin, 10, 100 };
			yield return new object[] { LayoutAlignment.Center, point, margin, 100, 100 };
			yield return new object[] { LayoutAlignment.End, point, margin, 190, 100 };
			yield return new object[] { LayoutAlignment.Fill, point, margin, 10, 280 };

			margin = new Thickness(50);
			yield return new object[] { LayoutAlignment.Center, point, margin, 100, 100 };
			yield return new object[] { LayoutAlignment.Start, point, margin, 50, 100 };
			yield return new object[] { LayoutAlignment.End, point, margin, 150, 100 };

			// Lopsided margin
			margin = new Thickness(5, 5, 10, 10);
			yield return new object[] { LayoutAlignment.Start, point, margin, 5, 100 };
			yield return new object[] { LayoutAlignment.Center, point, margin, 97.5, 100 };
			yield return new object[] { LayoutAlignment.End, point, margin, 190, 100 };
			yield return new object[] { LayoutAlignment.Fill, point, margin, 5, 285 };

			margin = new Thickness(100, 100, 0, 0);
			yield return new object[] { LayoutAlignment.Center, point, margin, 150, 100 };
			yield return new object[] { LayoutAlignment.Start, point, margin, 100, 100 };
			yield return new object[] { LayoutAlignment.End, point, margin, 200, 100 };

			margin = new Thickness(0, 0, 100, 100);
			yield return new object[] { LayoutAlignment.Center, point, margin, 50, 100 };
			yield return new object[] { LayoutAlignment.Start, point, margin, 0, 100 };
			yield return new object[] { LayoutAlignment.End, point, margin, 100, 100 };

			// X and Y offsets (e.g., GridLayout columns and rows)
			margin = Thickness.Zero;
			point = new Point(10, 10);
			yield return new object[] { LayoutAlignment.Start, point, margin, 10, 100 };
			yield return new object[] { LayoutAlignment.Center, point, margin, 110, 100 };
			yield return new object[] { LayoutAlignment.End, point, margin, 210, 100 };
			yield return new object[] { LayoutAlignment.Fill, point, margin, 10, 300 };
		}

		[Theory]
		[MemberData(nameof(AlignmentTestData))]
		public void FrameAccountsForHorizontalLayoutAlignment(LayoutAlignment layoutAlignment, Point offset, Thickness margin,
			double expectedX, double expectedWidth)
		{
			var widthConstraint = 300;
			var heightConstraint = 50;
			var viewSize = new Size(100, 50);
			var viewSizeIncludingMargins = new Size(viewSize.Width + margin.HorizontalThickness, viewSize.Height + margin.VerticalThickness);

			var element = Substitute.For<IView>();

			element.Margin.Returns(margin);
			element.DesiredSize.Returns(viewSizeIncludingMargins);
			element.HorizontalLayoutAlignment.Returns(layoutAlignment);
			element.Width.Returns(Dimension.Unset);
			element.Height.Returns(Dimension.Unset);
			element.MaximumWidth.Returns(Dimension.Maximum);
			element.MaximumHeight.Returns(Dimension.Maximum);
			element.FlowDirection.Returns(FlowDirection.LeftToRight);

			var frame = element.ComputeFrame(new Rect(offset.X, offset.Y, widthConstraint, heightConstraint));

			Assert.Equal(expectedX, frame.Left);
			Assert.Equal(expectedWidth, frame.Width);
		}

		[Theory]
		[MemberData(nameof(AlignmentTestData))]
		public void FrameAccountsForVerticalLayoutAlignment(LayoutAlignment layoutAlignment, Point offset, Thickness margin,
			double expectedY, double expectedHeight)
		{
			var widthConstraint = 50;
			var heightConstraint = 300;
			var viewSize = new Size(50, 100);
			var viewSizeIncludingMargins = new Size(viewSize.Width + margin.HorizontalThickness, viewSize.Height + margin.VerticalThickness);

			var element = Substitute.For<IView>();

			element.Margin.Returns(margin);
			element.DesiredSize.Returns(viewSizeIncludingMargins);
			element.VerticalLayoutAlignment.Returns(layoutAlignment);
			element.Width.Returns(Dimension.Unset);
			element.Height.Returns(Dimension.Unset);
			element.MaximumWidth.Returns(Dimension.Maximum);
			element.MaximumHeight.Returns(Dimension.Maximum);
			element.FlowDirection.Returns(FlowDirection.LeftToRight);

			var frame = element.ComputeFrame(new Rect(offset.X, offset.Y, widthConstraint, heightConstraint));

			Assert.Equal(expectedY, frame.Top);
			Assert.Equal(expectedHeight, frame.Height);
		}

		[Fact]
		public void WidthOverridesFill()
		{
			var widthConstraint = 300;
			var heightConstraint = 300;

			var viewWidth = 100;
			var desiredSize = new Size(viewWidth, 50);

			var element = Substitute.For<IView>();
			element.DesiredSize.Returns(desiredSize);
			element.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);
			element.Width.Returns(viewWidth);
			element.Height.Returns(Dimension.Unset);

			var frame = element.ComputeFrame(new Rect(0, 0, widthConstraint, heightConstraint));

			// Since a width was specified, it wins over the Fill and the width should end up being 100
			Assert.Equal(100, frame.Width);
		}

		[Fact]
		public void WidthOverridesFillFromCenter()
		{
			var widthConstraint = 300;
			var heightConstraint = 300;

			var viewWidth = 100;
			var desiredSize = new Size(viewWidth, 50);

			var element = Substitute.For<IView>();
			element.DesiredSize.Returns(desiredSize);
			element.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);
			element.Width.Returns(viewWidth);
			element.Height.Returns(Dimension.Unset);

			var frame = element.ComputeFrame(new Rect(0, 0, widthConstraint, heightConstraint));

			// Since a width was specified, it wins over the Fill
			// We want to do the filling from the center of the space, so the left edge of the frame should be
			// the center, minus half of the view
			var expectedX = (widthConstraint / 2) - (viewWidth / 2);

			Assert.Equal(expectedX, frame.Left);
		}

		[Fact]
		public void HeightOverridesFill()
		{
			var widthConstraint = 300;
			var heightConstraint = 300;

			var viewHeight = 100;
			var desiredSize = new Size(50, viewHeight);

			var element = Substitute.For<IView>();
			element.DesiredSize.Returns(desiredSize);
			element.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);
			element.Height.Returns(100);
			element.Width.Returns(Dimension.Unset);

			var frame = element.ComputeFrame(new Rect(0, 0, widthConstraint, heightConstraint));

			// Since a height was specified, it wins over the Fill and the height should end up being 100
			Assert.Equal(100, frame.Height);
		}

		[Fact]
		public void HeightOverridesFillFromCenter()
		{
			var widthConstraint = 300;
			var heightConstraint = 300;

			var viewHeight = 100;
			var desiredSize = new Size(50, viewHeight);

			var element = Substitute.For<IView>();
			element.DesiredSize.Returns(desiredSize);
			element.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);
			element.Height.Returns(viewHeight);
			element.Width.Returns(Dimension.Unset);

			var frame = element.ComputeFrame(new Rect(0, 0, widthConstraint, heightConstraint));

			// Since a height was specified, it wins over the Fill
			// We want to do the filling from the center of the space, so the top edge of the frame should be
			// the center, minus half of the view
			var expectedY = (heightConstraint / 2) - (viewHeight / 2);

			Assert.Equal(expectedY, frame.Top);
		}

		[Theory]
		[InlineData(0, 300)]
		[InlineData(300, 0)]
		[InlineData(Dimension.Maximum, 300)]
		[InlineData(Dimension.Maximum, 0)]
		public void HorizontalFillRespectsMaxWidth(double maxWidth, double widthConstraint)
		{
			var heightConstraint = 300;
			var desiredSize = new Size(50, 50);

			var element = Substitute.For<IView>();
			element.DesiredSize.Returns(desiredSize);
			element.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);
			element.Width.Returns(Dimension.Unset);
			element.MaximumWidth.Returns(maxWidth);

			var frame = element.ComputeFrame(new Rect(0, 0, widthConstraint, heightConstraint));

			// The width should always be the minimum between the width constraint and the element's MaximumWidth 
			var expectedWidth = Math.Min(maxWidth, widthConstraint);

			Assert.Equal(expectedWidth, frame.Width);
		}

		[Theory]
		[InlineData(0, 300)]
		[InlineData(300, 0)]
		[InlineData(Dimension.Maximum, 300)]
		[InlineData(Dimension.Maximum, 0)]
		public void VerticalFillRespectsMaxHeight(double maxHeight, double heightConstraint)
		{
			var widthConstraint = 300;
			var desiredSize = new Size(50, 50);

			var element = Substitute.For<IView>();
			element.DesiredSize.Returns(desiredSize);
			element.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);
			element.Height.Returns(Dimension.Unset);
			element.MaximumHeight.Returns(maxHeight);

			var frame = element.ComputeFrame(new Rect(0, 0, widthConstraint, heightConstraint));

			// The width should always be the minimum between the width constraint and the element's MaximumWidth
			var expectedHeight = Math.Min(maxHeight, heightConstraint);

			Assert.Equal(expectedHeight, frame.Height);
		}


		[Fact]
		public void MaxWidthOverridesFromCenter()
		{
			var widthConstraint = 300;
			var heightConstraint = 300;
			var desiredSize = new Size(50, 50);

			var maxWidth = 100;

			var element = Substitute.For<IView>();
			element.DesiredSize.Returns(desiredSize);
			element.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);
			element.Width.Returns(Dimension.Unset);
			element.MaximumWidth.Returns(maxWidth);

			var frame = element.ComputeFrame(new Rect(0, 0, widthConstraint, heightConstraint));

			// Since we set MaxWidth (and its less than the width constraint), our expected width should win over fill
			// We want to do the filling from the center of the space, so the top edge of the frame should be
			// the center, minus half of the view
			var expectedWidth = Math.Min(maxWidth, widthConstraint);
			var expectedX = (widthConstraint / 2) - (expectedWidth / 2);

			Assert.Equal(expectedX, frame.X);
		}

		[Fact]
		public void MaxHeightOverridesFromCenter()
		{
			var widthConstraint = 300;
			var heightConstraint = 300;
			var desiredSize = new Size(50, 50);

			var maxHeight = 100;

			var element = Substitute.For<IView>();
			element.DesiredSize.Returns(desiredSize);
			element.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);
			element.Height.Returns(Dimension.Unset);
			element.MaximumHeight.Returns(maxHeight);

			var frame = element.ComputeFrame(new Rect(0, 0, widthConstraint, heightConstraint));

			// Since we set MaxHeight (and its less than the height constraint), our expected height should win over fill
			// We want to do the filling from the center of the space, so the top edge of the frame should be
			// the center, minus half of the view
			var expectedHeight = Math.Min(maxHeight, heightConstraint);
			var expectedY = (heightConstraint / 2) - (expectedHeight / 2);

			Assert.Equal(expectedY, frame.Y);
		}
	}
}
