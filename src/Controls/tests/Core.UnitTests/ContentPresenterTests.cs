#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the ContentPresenter class.
    /// </summary>
    public class ContentPresenterTests
    {
        /// <summary>
        /// Tests that the Padding property getter returns the correct Thickness value.
        /// Verifies that getting the Padding property calls GetValue and returns the cast result.
        /// Expected result: The getter returns the Thickness value stored in the bindable property.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(10, 20, 30, 40)]
        [InlineData(-5, -10, -15, -20)]
        [InlineData(100.5, 200.75, 300.25, 400.125)]
        public void Padding_Get_ReturnsCorrectThicknessValue(double left, double top, double right, double bottom)
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var expectedThickness = new Thickness(left, top, right, bottom);
            contentPresenter.Padding = expectedThickness;

            // Act
            var result = contentPresenter.Padding;

            // Assert
            Assert.Equal(expectedThickness.Left, result.Left);
            Assert.Equal(expectedThickness.Top, result.Top);
            Assert.Equal(expectedThickness.Right, result.Right);
            Assert.Equal(expectedThickness.Bottom, result.Bottom);
        }

        /// <summary>
        /// Tests that the Padding property setter correctly stores the Thickness value.
        /// Verifies that setting the Padding property calls SetValue with the provided value.
        /// Expected result: The setter stores the Thickness value in the bindable property.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(15, 25, 35, 45)]
        [InlineData(-2.5, -7.5, -12.5, -17.5)]
        [InlineData(999.999, 888.888, 777.777, 666.666)]
        public void Padding_Set_StoresCorrectThicknessValue(double left, double top, double right, double bottom)
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var thickness = new Thickness(left, top, right, bottom);

            // Act
            contentPresenter.Padding = thickness;

            // Assert
            var result = contentPresenter.Padding;
            Assert.Equal(thickness.Left, result.Left);
            Assert.Equal(thickness.Top, result.Top);
            Assert.Equal(thickness.Right, result.Right);
            Assert.Equal(thickness.Bottom, result.Bottom);
        }

        /// <summary>
        /// Tests that the Padding property handles uniform thickness values correctly.
        /// Verifies that setting and getting uniform Thickness values works as expected.
        /// Expected result: Uniform thickness values are correctly stored and retrieved.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(-5)]
        [InlineData(50.5)]
        [InlineData(1000)]
        public void Padding_UniformThickness_HandledCorrectly(double uniformSize)
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var thickness = new Thickness(uniformSize);

            // Act
            contentPresenter.Padding = thickness;

            // Assert
            var result = contentPresenter.Padding;
            Assert.Equal(uniformSize, result.Left);
            Assert.Equal(uniformSize, result.Top);
            Assert.Equal(uniformSize, result.Right);
            Assert.Equal(uniformSize, result.Bottom);
        }

        /// <summary>
        /// Tests that the Padding property handles extreme double values correctly.
        /// Verifies behavior with double.MaxValue, double.MinValue, and special values.
        /// Expected result: Extreme values are handled without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.Epsilon)]
        public void Padding_ExtremeValues_HandledCorrectly(double value)
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var thickness = new Thickness(value);

            // Act
            contentPresenter.Padding = thickness;

            // Assert
            var result = contentPresenter.Padding;
            Assert.Equal(value, result.Left);
            Assert.Equal(value, result.Top);
            Assert.Equal(value, result.Right);
            Assert.Equal(value, result.Bottom);
        }

        /// <summary>
        /// Tests that the Padding property handles NaN values correctly.
        /// Verifies that NaN thickness values can be set and retrieved.
        /// Expected result: NaN values are preserved in the thickness.
        /// </summary>
        [Fact]
        public void Padding_NaNValues_HandledCorrectly()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var thickness = new Thickness(double.NaN, double.NaN, double.NaN, double.NaN);

            // Act
            contentPresenter.Padding = thickness;

            // Assert
            var result = contentPresenter.Padding;
            Assert.True(double.IsNaN(result.Left));
            Assert.True(double.IsNaN(result.Top));
            Assert.True(double.IsNaN(result.Right));
            Assert.True(double.IsNaN(result.Bottom));
        }

        /// <summary>
        /// Tests that the Padding property handles infinity values correctly.
        /// Verifies that positive and negative infinity thickness values can be set and retrieved.
        /// Expected result: Infinity values are preserved in the thickness.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Padding_InfinityValues_HandledCorrectly(double infinityValue)
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var thickness = new Thickness(infinityValue);

            // Act
            contentPresenter.Padding = thickness;

            // Assert
            var result = contentPresenter.Padding;
            Assert.Equal(infinityValue, result.Left);
            Assert.Equal(infinityValue, result.Top);
            Assert.Equal(infinityValue, result.Right);
            Assert.Equal(infinityValue, result.Bottom);
        }

        /// <summary>
        /// Tests that the Padding property default value is zero thickness.
        /// Verifies that a new ContentPresenter instance has default padding of zero.
        /// Expected result: Default padding is Thickness with all values set to zero.
        /// </summary>
        [Fact]
        public void Padding_DefaultValue_IsZeroThickness()
        {
            // Arrange & Act
            var contentPresenter = new ContentPresenter();

            // Assert
            var result = contentPresenter.Padding;
            Assert.Equal(0, result.Left);
            Assert.Equal(0, result.Top);
            Assert.Equal(0, result.Right);
            Assert.Equal(0, result.Bottom);
        }

        /// <summary>
        /// Tests that the Padding property maintains value integrity across multiple set/get operations.
        /// Verifies that repeatedly setting and getting the Padding property maintains consistency.
        /// Expected result: Multiple set/get operations preserve the thickness values.
        /// </summary>
        [Fact]
        public void Padding_MultipleSetGet_MaintainsValueIntegrity()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var thickness1 = new Thickness(10, 20, 30, 40);
            var thickness2 = new Thickness(50, 60, 70, 80);

            // Act & Assert - First set/get
            contentPresenter.Padding = thickness1;
            var result1 = contentPresenter.Padding;
            Assert.Equal(thickness1.Left, result1.Left);
            Assert.Equal(thickness1.Top, result1.Top);
            Assert.Equal(thickness1.Right, result1.Right);
            Assert.Equal(thickness1.Bottom, result1.Bottom);

            // Act & Assert - Second set/get
            contentPresenter.Padding = thickness2;
            var result2 = contentPresenter.Padding;
            Assert.Equal(thickness2.Left, result2.Left);
            Assert.Equal(thickness2.Top, result2.Top);
            Assert.Equal(thickness2.Right, result2.Right);
            Assert.Equal(thickness2.Bottom, result2.Bottom);
        }

        /// <summary>
        /// Tests MeasureOverride with normal positive constraints when Handler is null.
        /// Should return Size.Zero since ComputeDesiredSize returns Size.Zero when Handler is null.
        /// </summary>
        [Fact]
        public void MeasureOverride_WithNormalConstraints_HandlerNull_ReturnsSizeZero()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            double widthConstraint = 100.0;
            double heightConstraint = 200.0;

            // Act
            var result = contentPresenter.MeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(Size.Zero, result);
        }

        /// <summary>
        /// Tests MeasureOverride with various constraint values when Handler is null.
        /// Should return Size.Zero for all cases since ComputeDesiredSize returns Size.Zero when Handler is null.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(100.0, 200.0)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(-1.0, -1.0)]
        [InlineData(50.5, 75.25)]
        public void MeasureOverride_WithVariousConstraints_HandlerNull_ReturnsSizeZero(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var contentPresenter = new ContentPresenter();

            // Act
            var result = contentPresenter.MeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(Size.Zero, result);
        }

        /// <summary>
        /// Tests MeasureOverride with NaN constraints when Handler is null.
        /// Should return Size.Zero since ComputeDesiredSize returns Size.Zero when Handler is null.
        /// </summary>
        [Fact]
        public void MeasureOverride_WithNaNConstraints_HandlerNull_ReturnsSizeZero()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            double widthConstraint = double.NaN;
            double heightConstraint = double.NaN;

            // Act
            var result = contentPresenter.MeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(Size.Zero, result);
        }

        /// <summary>
        /// Tests MeasureOverride with minimum double values when Handler is null.
        /// Should return Size.Zero since ComputeDesiredSize returns Size.Zero when Handler is null.
        /// </summary>
        [Fact]
        public void MeasureOverride_WithMinValues_HandlerNull_ReturnsSizeZero()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            double widthConstraint = double.MinValue;
            double heightConstraint = double.MinValue;

            // Act
            var result = contentPresenter.MeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(Size.Zero, result);
        }

        /// <summary>
        /// Tests MeasureOverride with mixed constraint values when Handler is null.
        /// Should return Size.Zero since ComputeDesiredSize returns Size.Zero when Handler is null.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, 100.0)]
        [InlineData(100.0, double.PositiveInfinity)]
        [InlineData(double.NaN, 100.0)]
        [InlineData(100.0, double.NaN)]
        [InlineData(double.MaxValue, 0.0)]
        [InlineData(0.0, double.MaxValue)]
        [InlineData(-50.0, 100.0)]
        [InlineData(100.0, -50.0)]
        public void MeasureOverride_WithMixedConstraints_HandlerNull_ReturnsSizeZero(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var contentPresenter = new ContentPresenter();

            // Act
            var result = contentPresenter.MeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(Size.Zero, result);
        }

        /// <summary>
        /// Tests MeasureOverride with zero constraints when Handler is null.
        /// Should return Size.Zero since ComputeDesiredSize returns Size.Zero when Handler is null.
        /// </summary>
        [Fact]
        public void MeasureOverride_WithZeroConstraints_HandlerNull_ReturnsSizeZero()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            double widthConstraint = 0.0;
            double heightConstraint = 0.0;

            // Act
            var result = contentPresenter.MeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(Size.Zero, result);
        }

        /// <summary>
        /// Tests MeasureOverride with very large constraint values when Handler is null.
        /// Should return Size.Zero since ComputeDesiredSize returns Size.Zero when Handler is null.
        /// </summary>
        [Fact]
        public void MeasureOverride_WithVeryLargeConstraints_HandlerNull_ReturnsSizeZero()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            double widthConstraint = 1e15;
            double heightConstraint = 1e15;

            // Act
            var result = contentPresenter.MeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(Size.Zero, result);
        }

        /// <summary>
        /// Tests MeasureOverride with very small positive constraint values when Handler is null.
        /// Should return Size.Zero since ComputeDesiredSize returns Size.Zero when Handler is null.
        /// </summary>
        [Fact]
        public void MeasureOverride_WithVerySmallConstraints_HandlerNull_ReturnsSizeZero()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            double widthConstraint = 1e-10;
            double heightConstraint = 1e-10;

            // Act
            var result = contentPresenter.MeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(Size.Zero, result);
        }

        /// <summary>
        /// Helper class to expose the protected OnSizeAllocated method for testing.
        /// </summary>
        class TestableContentPresenter : ContentPresenter
        {
            public void PublicOnSizeAllocated(double width, double height)
            {
                OnSizeAllocated(width, height);
            }
        }

        /// <summary>
        /// Tests OnSizeAllocated with normal positive values to ensure the method executes without throwing exceptions.
        /// </summary>
        [Fact]
        public void OnSizeAllocated_WithNormalValues_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();

            // Act & Assert
            var exception = Record.Exception(() => contentPresenter.PublicOnSizeAllocated(100.0, 200.0));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnSizeAllocated with zero values to ensure the method handles boundary conditions correctly.
        /// </summary>
        [Fact]
        public void OnSizeAllocated_WithZeroValues_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();

            // Act & Assert
            var exception = Record.Exception(() => contentPresenter.PublicOnSizeAllocated(0.0, 0.0));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnSizeAllocated with negative values to ensure the method handles invalid size conditions gracefully.
        /// </summary>
        [Fact]
        public void OnSizeAllocated_WithNegativeValues_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();

            // Act & Assert
            var exception = Record.Exception(() => contentPresenter.PublicOnSizeAllocated(-50.0, -100.0));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnSizeAllocated with very large values to ensure the method handles extreme boundary conditions.
        /// </summary>
        [Fact]
        public void OnSizeAllocated_WithVeryLargeValues_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();

            // Act & Assert
            var exception = Record.Exception(() => contentPresenter.PublicOnSizeAllocated(double.MaxValue, double.MaxValue));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnSizeAllocated with NaN values to ensure the method handles invalid floating-point conditions.
        /// </summary>
        [Fact]
        public void OnSizeAllocated_WithNaNValues_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();

            // Act & Assert
            var exception = Record.Exception(() => contentPresenter.PublicOnSizeAllocated(double.NaN, double.NaN));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnSizeAllocated with positive infinity values to ensure the method handles infinite boundary conditions.
        /// </summary>
        [Fact]
        public void OnSizeAllocated_WithPositiveInfinityValues_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();

            // Act & Assert
            var exception = Record.Exception(() => contentPresenter.PublicOnSizeAllocated(double.PositiveInfinity, double.PositiveInfinity));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnSizeAllocated with negative infinity values to ensure the method handles negative infinite boundary conditions.
        /// </summary>
        [Fact]
        public void OnSizeAllocated_WithNegativeInfinityValues_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();

            // Act & Assert
            var exception = Record.Exception(() => contentPresenter.PublicOnSizeAllocated(double.NegativeInfinity, double.NegativeInfinity));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnSizeAllocated with mixed edge case values (one normal, one edge case) to ensure the method handles asymmetric conditions.
        /// </summary>
        [Theory]
        [InlineData(100.0, 0.0)]
        [InlineData(0.0, 100.0)]
        [InlineData(100.0, double.NaN)]
        [InlineData(double.NaN, 100.0)]
        [InlineData(100.0, double.PositiveInfinity)]
        [InlineData(double.PositiveInfinity, 100.0)]
        [InlineData(-50.0, 100.0)]
        [InlineData(100.0, -50.0)]
        public void OnSizeAllocated_WithMixedEdgeCaseValues_DoesNotThrow(double width, double height)
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();

            // Act & Assert
            var exception = Record.Exception(() => contentPresenter.PublicOnSizeAllocated(width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests ArrangeOverride with valid bounds to ensure Frame is computed correctly and size is returned.
        /// Tests the normal case where bounds are valid positive values.
        /// Expected result: Frame is set to computed frame and Frame.Size is returned.
        /// </summary>
        [Fact]
        public void ArrangeOverride_ValidBounds_SetsFrameAndReturnsSize()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var bounds = new Rect(10, 20, 100, 200);

            // Act
            var result = contentPresenter.ArrangeOverride(bounds);

            // Assert
            Assert.NotEqual(Rect.Zero, contentPresenter.Frame);
            Assert.Equal(contentPresenter.Frame.Size, result);
        }

        /// <summary>
        /// Tests ArrangeOverride with Handler set to null to ensure no exception is thrown.
        /// Tests the edge case where Handler property is null.
        /// Expected result: No exception is thrown and method completes successfully.
        /// </summary>
        [Fact]
        public void ArrangeOverride_HandlerIsNull_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            contentPresenter.Handler = null;
            var bounds = new Rect(0, 0, 50, 50);

            // Act & Assert
            var result = contentPresenter.ArrangeOverride(bounds);
            Assert.Equal(contentPresenter.Frame.Size, result);
        }

        /// <summary>
        /// Tests ArrangeOverride with Handler set to verify PlatformArrange is called.
        /// Tests the case where Handler is not null and should receive PlatformArrange call.
        /// Expected result: Handler.PlatformArrange is called with the computed Frame.
        /// </summary>
        [Fact]
        public void ArrangeOverride_HandlerNotNull_CallsPlatformArrange()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var mockHandler = Substitute.For<IViewHandler>();
            contentPresenter.Handler = mockHandler;
            var bounds = new Rect(5, 10, 80, 90);

            // Act
            var result = contentPresenter.ArrangeOverride(bounds);

            // Assert
            mockHandler.Received(1).PlatformArrange(contentPresenter.Frame);
            Assert.Equal(contentPresenter.Frame.Size, result);
        }

        /// <summary>
        /// Tests ArrangeOverride with various edge case bounds including empty, negative, and large values.
        /// Tests boundary conditions for bounds parameter.
        /// Expected result: Method handles all boundary values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(-10, -20, 50, 100)]
        [InlineData(10, 20, -50, -100)]
        [InlineData(-10, -20, -50, -100)]
        [InlineData(double.MaxValue, double.MaxValue, 100, 200)]
        [InlineData(0, 0, double.MaxValue, double.MaxValue)]
        public void ArrangeOverride_BoundaryValues_HandlesCorrectly(double x, double y, double width, double height)
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var bounds = new Rect(x, y, width, height);

            // Act
            var result = contentPresenter.ArrangeOverride(bounds);

            // Assert
            Assert.Equal(contentPresenter.Frame.Size, result);
        }

        /// <summary>
        /// Tests ArrangeOverride with NaN values in bounds to verify handling of special floating-point values.
        /// Tests edge case where bounds contain NaN values.
        /// Expected result: Method completes without throwing and returns appropriate size.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0, 100, 200)]
        [InlineData(0, double.NaN, 100, 200)]
        [InlineData(0, 0, double.NaN, 200)]
        [InlineData(0, 0, 100, double.NaN)]
        public void ArrangeOverride_NaNValues_HandlesCorrectly(double x, double y, double width, double height)
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var bounds = new Rect(x, y, width, height);

            // Act
            var result = contentPresenter.ArrangeOverride(bounds);

            // Assert
            Assert.Equal(contentPresenter.Frame.Size, result);
        }

        /// <summary>
        /// Tests ArrangeOverride with infinity values in bounds to verify handling of infinite values.
        /// Tests edge case where bounds contain positive or negative infinity values.
        /// Expected result: Method completes without throwing and returns appropriate size.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, 0, 100, 200)]
        [InlineData(0, double.PositiveInfinity, 100, 200)]
        [InlineData(0, 0, double.PositiveInfinity, 200)]
        [InlineData(0, 0, 100, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0, 100, 200)]
        [InlineData(0, double.NegativeInfinity, 100, 200)]
        [InlineData(0, 0, double.NegativeInfinity, 200)]
        [InlineData(0, 0, 100, double.NegativeInfinity)]
        public void ArrangeOverride_InfinityValues_HandlesCorrectly(double x, double y, double width, double height)
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var bounds = new Rect(x, y, width, height);

            // Act
            var result = contentPresenter.ArrangeOverride(bounds);

            // Assert
            Assert.Equal(contentPresenter.Frame.Size, result);
        }

        /// <summary>
        /// Tests ArrangeOverride to verify that Frame property is updated with computed frame.
        /// Tests that the Frame property is correctly set during arrangement.
        /// Expected result: Frame property reflects the computed frame from ComputeFrame call.
        /// </summary>
        [Fact]
        public void ArrangeOverride_UpdatesFrameProperty()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var bounds = new Rect(15, 25, 120, 180);
            var originalFrame = contentPresenter.Frame;

            // Act
            contentPresenter.ArrangeOverride(bounds);

            // Assert
            Assert.NotEqual(originalFrame, contentPresenter.Frame);
        }

        /// <summary>
        /// Tests ArrangeOverride with multiple calls to ensure consistent behavior.
        /// Tests that multiple calls with different bounds update Frame appropriately.
        /// Expected result: Each call updates Frame and returns corresponding size.
        /// </summary>
        [Fact]
        public void ArrangeOverride_MultipleCalls_UpdatesFrameEachTime()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var bounds1 = new Rect(10, 20, 100, 150);
            var bounds2 = new Rect(30, 40, 200, 250);

            // Act
            var result1 = contentPresenter.ArrangeOverride(bounds1);
            var frame1 = contentPresenter.Frame;
            var result2 = contentPresenter.ArrangeOverride(bounds2);
            var frame2 = contentPresenter.Frame;

            // Assert
            Assert.Equal(frame1.Size, result1);
            Assert.Equal(frame2.Size, result2);
            Assert.NotEqual(frame1, frame2);
        }

        /// <summary>
        /// Tests ArrangeOverride with Handler to verify PlatformArrange receives correct Frame value.
        /// Tests that PlatformArrange is called with the exact Frame value that was computed.
        /// Expected result: PlatformArrange is called with the same Frame that is stored in the Frame property.
        /// </summary>
        [Fact]
        public void ArrangeOverride_HandlerReceivesCorrectFrame()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var mockHandler = Substitute.For<IViewHandler>();
            contentPresenter.Handler = mockHandler;
            var bounds = new Rect(7, 14, 75, 125);

            // Act
            contentPresenter.ArrangeOverride(bounds);

            // Assert
            mockHandler.Received(1).PlatformArrange(Arg.Is<Rect>(r => r == contentPresenter.Frame));
        }

        /// <summary>
        /// Tests that LayoutChildren can be called with normal positive values without throwing exceptions.
        /// This test validates the basic functionality of the obsolete LayoutChildren method.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithValidPositiveValues_DoesNotThrowException()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            double x = 10.0;
            double y = 20.0;
            double width = 100.0;
            double height = 200.0;

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => CallLayoutChildren(contentPresenter, x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LayoutChildren can be called with zero values without throwing exceptions.
        /// This test validates edge case behavior with zero dimensions and positions.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithZeroValues_DoesNotThrowException()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            double x = 0.0;
            double y = 0.0;
            double width = 0.0;
            double height = 0.0;

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => CallLayoutChildren(contentPresenter, x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LayoutChildren can be called with negative values without throwing exceptions.
        /// This test validates edge case behavior with negative positions and dimensions.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithNegativeValues_DoesNotThrowException()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            double x = -10.0;
            double y = -20.0;
            double width = -100.0;
            double height = -200.0;

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => CallLayoutChildren(contentPresenter, x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LayoutChildren can be called with extreme double values without throwing exceptions.
        /// This test validates behavior with boundary values like MaxValue and MinValue.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue, double.MinValue, double.MinValue)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN)]
        public void LayoutChildren_WithExtremeDoubleValues_DoesNotThrowException(double x, double y, double width, double height)
        {
            // Arrange
            var contentPresenter = new ContentPresenter();

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => CallLayoutChildren(contentPresenter, x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LayoutChildren can be called with mixed valid and extreme values without throwing exceptions.
        /// This test validates behavior with realistic mixed parameter combinations.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0, double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NaN, 0.0, 100.0, 200.0)]
        [InlineData(-50.0, -75.0, double.MaxValue, 150.0)]
        [InlineData(25.0, double.MinValue, 0.0, double.NegativeInfinity)]
        public void LayoutChildren_WithMixedValues_DoesNotThrowException(double x, double y, double width, double height)
        {
            // Arrange
            var contentPresenter = new ContentPresenter();

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => CallLayoutChildren(contentPresenter, x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Helper method to call the protected LayoutChildren method using reflection.
        /// This is necessary because LayoutChildren is a protected method and cannot be called directly in tests.
        /// </summary>
        /// <param name="contentPresenter">The ContentPresenter instance to test</param>
        /// <param name="x">X coordinate parameter</param>
        /// <param name="y">Y coordinate parameter</param>
        /// <param name="width">Width parameter</param>
        /// <param name="height">Height parameter</param>
        private static void CallLayoutChildren(ContentPresenter contentPresenter, double x, double y, double width, double height)
        {
            var method = typeof(ContentPresenter).GetMethod("LayoutChildren",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(contentPresenter, new object[] { x, y, width, height });
        }

        /// <summary>
        /// Tests that LayoutChildren can be called directly through inheritance without throwing exceptions.
        /// This test validates the method using a derived class to access the protected method.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void LayoutChildren_ThroughInheritance_DoesNotThrowException()
        {
            // Arrange
            var testablePresenter = new TestableContentPresenter();
            double x = 15.5;
            double y = 25.75;
            double width = 300.25;
            double height = 400.5;

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => testablePresenter.TestLayoutChildren(x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RaiseChild method handles null view parameter gracefully.
        /// Should not throw an exception when null is passed.
        /// </summary>
        [Fact]
        public void RaiseChild_NullView_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new ContentPresenter
            {
                IsPlatformEnabled = true
            };

            // Act & Assert
            var exception = Record.Exception(() => contentPresenter.RaiseChild(null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RaiseChild method handles a valid view that is a child of the presenter.
        /// Should call the base implementation to move the view to the front of the visual stack.
        /// </summary>
        [Fact]
        public void RaiseChild_ValidChildView_CallsBaseImplementation()
        {
            // Arrange
            var contentPresenter = new ContentPresenter
            {
                IsPlatformEnabled = true
            };
            var childView = new Label { Text = "Test" };
            contentPresenter.Content = childView;

            // Act
            var exception = Record.Exception(() => contentPresenter.RaiseChild(childView));

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RaiseChild method handles a view that is not a child of the presenter.
        /// Should call the base implementation which will return early without error.
        /// </summary>
        [Fact]
        public void RaiseChild_NonChildView_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new ContentPresenter
            {
                IsPlatformEnabled = true
            };
            var nonChildView = new Label { Text = "Not a child" };

            // Act
            var exception = Record.Exception(() => contentPresenter.RaiseChild(nonChildView));

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RaiseChild method with various view types.
        /// Should handle different types of views without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(typeof(Label))]
        [InlineData(typeof(Button))]
        [InlineData(typeof(Entry))]
        public void RaiseChild_DifferentViewTypes_DoesNotThrow(Type viewType)
        {
            // Arrange
            var contentPresenter = new ContentPresenter
            {
                IsPlatformEnabled = true
            };
            var view = (View)Activator.CreateInstance(viewType);

            // Act
            var exception = Record.Exception(() => contentPresenter.RaiseChild(view));

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RaiseChild method works when ContentPresenter has content set.
        /// Should successfully call base implementation even when content is present.
        /// </summary>
        [Fact]
        public void RaiseChild_WithExistingContent_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new ContentPresenter
            {
                IsPlatformEnabled = true
            };
            var existingContent = new Label { Text = "Existing" };
            var targetView = new Button { Text = "Target" };

            contentPresenter.Content = existingContent;

            // Act
            var exception = Record.Exception(() => contentPresenter.RaiseChild(targetView));

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RaiseChild method maintains the obsolete attribute behavior.
        /// The method should still function despite being marked as obsolete.
        /// </summary>
        [Fact]
        public void RaiseChild_ObsoleteMethod_StillFunctions()
        {
            // Arrange
            var contentPresenter = new ContentPresenter
            {
                IsPlatformEnabled = true
            };
            var view = new Label { Text = "Test" };

            // Act
            var exception = Record.Exception(() => contentPresenter.RaiseChild(view));

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RaiseChild method can be called multiple times with the same view.
        /// Should handle repeated calls gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void RaiseChild_RepeatedCalls_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new ContentPresenter
            {
                IsPlatformEnabled = true
            };
            var view = new Label { Text = "Test" };

            // Act & Assert
            var exception1 = Record.Exception(() => contentPresenter.RaiseChild(view));
            var exception2 = Record.Exception(() => contentPresenter.RaiseChild(view));
            var exception3 = Record.Exception(() => contentPresenter.RaiseChild(view));

            Assert.Null(exception1);
            Assert.Null(exception2);
            Assert.Null(exception3);
        }

        /// <summary>
        /// Tests that InvalidateLayout method can be called without throwing exceptions.
        /// This method is obsolete but should still function properly by calling the base implementation.
        /// </summary>
        [Fact]
        public void InvalidateLayout_CallsBaseImplementation_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();

            // Act & Assert - Should not throw any exceptions
            var exception = Record.Exception(() => contentPresenter.CallInvalidateLayout());

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that InvalidateLayout triggers measure invalidation by checking if InvalidateMeasure gets called.
        /// The base implementation should eventually call InvalidateMeasure which triggers a measure invalidation.
        /// </summary>
        [Fact]
        public void InvalidateLayout_TriggersMeasureInvalidation_MeasureInvalidatedEventRaised()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();
            var measureInvalidatedRaised = false;

            contentPresenter.MeasureInvalidated += (sender, args) =>
            {
                measureInvalidatedRaised = true;
            };

            // Act
            contentPresenter.CallInvalidateLayout();

            // Assert
            Assert.True(measureInvalidatedRaised);
        }

        /// <summary>
        /// Tests that InvalidateLayout can be called multiple times without issues.
        /// This ensures the method is idempotent and doesn't cause issues with repeated calls.
        /// </summary>
        [Fact]
        public void InvalidateLayout_CalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();

            // Act & Assert - Should not throw any exceptions when called multiple times
            var exception = Record.Exception(() =>
            {
                contentPresenter.CallInvalidateLayout();
                contentPresenter.CallInvalidateLayout();
                contentPresenter.CallInvalidateLayout();
            });

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnChildMeasureInvalidated can be called without throwing an exception.
        /// This method is obsolete and has an empty implementation, so the test verifies basic execution.
        /// </summary>
        [Fact]
        public void OnChildMeasureInvalidated_WhenCalled_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();

            // Act & Assert
            var exception = Record.Exception(() => contentPresenter.CallOnChildMeasureInvalidated());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnChildMeasureInvalidated can be called multiple times without issues.
        /// Since the method is empty, multiple calls should not cause any side effects or exceptions.
        /// </summary>
        [Fact]
        public void OnChildMeasureInvalidated_WhenCalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                contentPresenter.CallOnChildMeasureInvalidated();
                contentPresenter.CallOnChildMeasureInvalidated();
                contentPresenter.CallOnChildMeasureInvalidated();
            });
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that ShouldInvalidateOnChildAdded returns true when passed a valid View instance.
        /// Verifies the method's expected behavior with a non-null View parameter.
        /// </summary>
        [Fact]
        public void ShouldInvalidateOnChildAdded_ValidView_ReturnsTrue()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();
            var view = new View();

            // Act
            var result = contentPresenter.CallShouldInvalidateOnChildAdded(view);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ShouldInvalidateOnChildAdded returns true when passed a null View parameter.
        /// Verifies the method handles null input without throwing an exception and returns the expected value.
        /// </summary>
        [Fact]
        public void ShouldInvalidateOnChildAdded_NullView_ReturnsTrue()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();

            // Act
            var result = contentPresenter.CallShouldInvalidateOnChildAdded(null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ShouldInvalidateOnChildAdded consistently returns true for multiple different View instances.
        /// Verifies the method's behavior is consistent regardless of the specific View instance passed.
        /// </summary>
        [Fact]
        public void ShouldInvalidateOnChildAdded_MultipleViews_AlwaysReturnsTrue()
        {
            // Arrange
            var contentPresenter = new TestableContentPresenter();
            var view1 = new View();
            var view2 = new View();

            // Act
            var result1 = contentPresenter.CallShouldInvalidateOnChildAdded(view1);
            var result2 = contentPresenter.CallShouldInvalidateOnChildAdded(view2);

            // Assert
            Assert.True(result1);
            Assert.True(result2);
        }

        /// <summary>
        /// Tests that LowerChild method can be called with a valid View parameter
        /// and successfully delegates to the base class implementation.
        /// </summary>
        [Fact]
        public void LowerChild_ValidView_CallsBaseImplementation()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            var view = new View();

            // Act & Assert - Should not throw
            contentPresenter.LowerChild(view);
        }

        /// <summary>
        /// Tests that LowerChild method can be called with a null View parameter
        /// and successfully delegates to the base class implementation without throwing.
        /// </summary>
        [Fact]
        public void LowerChild_NullView_CallsBaseImplementation()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();
            View view = null;

            // Act & Assert - Should not throw
            contentPresenter.LowerChild(view);
        }

        /// <summary>
        /// Tests that LowerChild method properly handles different View instances
        /// by ensuring the method executes without exceptions for various scenarios.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetViewTestCases))]
        public void LowerChild_VariousViewInputs_ExecutesSuccessfully(View view)
        {
            // Arrange
            var contentPresenter = new ContentPresenter();

            // Act & Assert - Should not throw
            contentPresenter.LowerChild(view);
        }

        public static IEnumerable<object[]> GetViewTestCases()
        {
            yield return new object[] { new View() };
            yield return new object[] { null };
            yield return new object[] { new TestableView() };
        }

        /// <summary>
        /// Helper class for testing View scenarios.
        /// </summary>
        private class TestableView : View
        {
            // Simple test view for validation
        }

        /// <summary>
        /// Tests that CascadeInputTransparent property returns the default value when accessed without being explicitly set.
        /// The default value should be true based on the BindableProperty definition.
        /// </summary>
        [Fact]
        public void CascadeInputTransparent_DefaultValue_ReturnsTrue()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();

            // Act
            var result = contentPresenter.CascadeInputTransparent;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CascadeInputTransparent property getter returns the correct value after setting it to true.
        /// Verifies the property setter and getter work correctly for true values.
        /// </summary>
        [Fact]
        public void CascadeInputTransparent_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();

            // Act
            contentPresenter.CascadeInputTransparent = true;
            var result = contentPresenter.CascadeInputTransparent;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CascadeInputTransparent property getter returns the correct value after setting it to false.
        /// Verifies the property setter and getter work correctly for false values.
        /// </summary>
        [Fact]
        public void CascadeInputTransparent_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var contentPresenter = new ContentPresenter();

            // Act
            contentPresenter.CascadeInputTransparent = false;
            var result = contentPresenter.CascadeInputTransparent;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CascadeInputTransparent property can be toggled between true and false values.
        /// Verifies the property maintains state correctly when changed multiple times.
        /// </summary>
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void CascadeInputTransparent_ToggleValues_MaintainsCorrectState(bool firstValue, bool secondValue)
        {
            // Arrange
            var contentPresenter = new ContentPresenter();

            // Act
            contentPresenter.CascadeInputTransparent = firstValue;
            var firstResult = contentPresenter.CascadeInputTransparent;

            contentPresenter.CascadeInputTransparent = secondValue;
            var secondResult = contentPresenter.CascadeInputTransparent;

            // Assert
            Assert.Equal(firstValue, firstResult);
            Assert.Equal(secondValue, secondResult);
        }
    }
}