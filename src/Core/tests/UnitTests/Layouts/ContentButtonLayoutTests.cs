using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.UnitTests.Layouts
{
	[Category(TestCategory.Core, TestCategory.Layout)]
	public class ContentButtonLayoutTests
	{
		static IContentButton CreateButton()
		{
			var button = Substitute.For<IContentButton>();
			button.Width.Returns(Dimension.Unset);
			button.Height.Returns(Dimension.Unset);
			button.MaximumWidth.Returns(double.PositiveInfinity);
			button.MaximumHeight.Returns(double.PositiveInfinity);
			button.StrokeThickness.Returns(-1);
			return button;
		}

		[Theory]
		[InlineData(-1, 26, 34)]
		[InlineData(0, 26, 34)]
		[InlineData(3, 32, 40)]
		public void AsymmetricInsetsAppliedOnce(double stroke, double expectedWidth, double expectedHeight)
		{
			var button = CreateButton();
			var child = Substitute.For<IView>();
			button.PresentedContent.Returns(child);
			button.Padding.Returns(new Thickness(1, 2, 5, 12));
			button.StrokeThickness.Returns(stroke);
			child.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(new Size(20, 20));

			Assert.Equal(new Size(expectedWidth, expectedHeight), ContentButtonLayout.Measure(button, 100, 100));
			var inset = ContentButtonLayout.GetInset(button);
			child.Received().Measure(100 - inset.HorizontalThickness, 100 - inset.VerticalThickness);
			ContentButtonLayout.Arrange(button, new Rect(10, 20, 100, 100));
			child.Received().Arrange(new Rect(10 + inset.Left, 20 + inset.Top,
				100 - inset.HorizontalThickness, 100 - inset.VerticalThickness));
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(10)]
		public void TinyConstraintsNeverPassNegativeSize(double constraint)
		{
			var button = CreateButton();
			var child = Substitute.For<IView>();
			button.PresentedContent.Returns(child);
			button.Padding.Returns(new Thickness(10));
			button.StrokeThickness.Returns(2);
			var size = ContentButtonLayout.Measure(button, constraint, constraint);
			child.Received().Measure(0, 0);
			Assert.Equal(new Size(Math.Max(0, constraint), Math.Max(0, constraint)), size);
			ContentButtonLayout.Arrange(button, new Rect(0, 0, constraint, constraint));
			child.Received().Arrange(new Rect(12, 12, 0, 0));
		}

		[Fact]
		public void InfiniteConstraintsReachChild()
		{
			var button = CreateButton();
			var child = Substitute.For<IView>();
			button.PresentedContent.Returns(child);
			child.Measure(double.PositiveInfinity, double.PositiveInfinity).Returns(new Size(80, 40));
			Assert.Equal(new Size(80, 40), ContentButtonLayout.Measure(button, double.PositiveInfinity, double.PositiveInfinity));
		}

		[Fact]
		public void MaximumDimensionsConstrainChildBeforeMeasuring()
		{
			var button = CreateButton();
			var child = Substitute.For<IView>();
			button.PresentedContent.Returns(child);
			button.Padding.Returns(new Thickness(5));
			button.MaximumWidth.Returns(80);
			button.MaximumHeight.Returns(40);
			child.Measure(70, 30).Returns(new Size(70, 30));
			Assert.Equal(new Size(80, 40), ContentButtonLayout.Measure(button, double.PositiveInfinity, double.PositiveInfinity));
			child.Received().Measure(70, 30);
		}

		[Fact]
		public void RequestedDimensionsAndMinimumsAreResolved()
		{
			var button = CreateButton();
			var child = Substitute.For<IView>();
			button.PresentedContent.Returns(child);
			button.Width.Returns(80);
			button.Height.Returns(40);
			button.MinimumWidth.Returns(90);
			button.MaximumHeight.Returns(30);
			Assert.Equal(new Size(90, 30), ContentButtonLayout.Measure(button, 200, 200));
			child.Received().Measure(80, 30);
		}

		[Fact]
		public void ExternalConstraintsLimitRequestedSize()
		{
			var button = CreateButton();
			var child = Substitute.For<IView>();
			button.PresentedContent.Returns(child);
			button.Width.Returns(200);
			button.Height.Returns(200);
			button.Padding.Returns(new Thickness(1, 2, 3, 4));
			button.StrokeThickness.Returns(2);
			Assert.Equal(new Size(50, 60), ContentButtonLayout.Measure(button, 50, 60));
			child.Received().Measure(42, 50);
		}

		[Fact]
		public void ConstrainedRequestedWidthPreservesWrappedContentHeight()
		{
			var button = CreateButton();
			var child = Substitute.For<IView>();
			button.PresentedContent.Returns(child);
			button.Width.Returns(200);
			button.Padding.Returns(new Thickness(1, 2, 3, 4));
			button.StrokeThickness.Returns(2);
			child.Measure(42, double.PositiveInfinity).Returns(new Size(42, 80));

			Assert.Equal(new Size(50, 90), ContentButtonLayout.Measure(button, 50, double.PositiveInfinity));
			child.Received().Measure(42, double.PositiveInfinity);
		}

		[Fact]
		public void EmptyContentStillAccountsForInsetAndMinimumSize()
		{
			var button = CreateButton();
			button.Padding.Returns(new Thickness(1, 2, 3, 4));
			button.MinimumWidth.Returns(30);
			Assert.Equal(new Size(30, 6), ContentButtonLayout.Measure(button, 100, 100));
			Assert.Equal(new Size(100, 100), ContentButtonLayout.Arrange(button, new Rect(0, 0, 100, 100)));
		}

		[Fact]
		public void ContentReplacementAndResizingDoNotUseStaleMeasurements()
		{
			var button = CreateButton();
			var first = Substitute.For<IView>();
			var second = Substitute.For<IView>();
			button.PresentedContent.Returns(first);
			first.Measure(100, 100).Returns(new Size(20, 10));
			Assert.Equal(new Size(20, 10), ContentButtonLayout.Measure(button, 100, 100));
			first.Measure(100, 100).Returns(new Size(30, 15));
			Assert.Equal(new Size(30, 15), ContentButtonLayout.Measure(button, 100, 100));
			button.PresentedContent.Returns(second);
			second.Measure(100, 100).Returns(new Size(40, 25));
			Assert.Equal(new Size(40, 25), ContentButtonLayout.Measure(button, 100, 100));
			button.PresentedContent.Returns((IView)null);
			Assert.Equal(Size.Zero, ContentButtonLayout.Measure(button, 100, 100));
		}
	}
}
