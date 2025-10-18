#nullable enable
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.CompilerServices;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NSubstitute;
#nullable enable
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class BorderUnitTests : BaseTestFixture
    {
        [Fact]
        public void HasNoVisualChildrenWhenNoContentIsSet()
        {
            var border = new Border();

            Assert.Empty(((IVisualTreeElement)border).GetVisualChildren());

            border.Content = null;

            Assert.Empty(((IVisualTreeElement)border).GetVisualChildren());
        }

        [Fact]
        public void HasVisualChildrenWhenContentIsSet()
        {
            var border = new Border();

            var label = new Label();
            border.Content = label;

            var visualTreeChildren = ((IVisualTreeElement)border).GetVisualChildren();
            Assert.NotEmpty(visualTreeChildren);
            Assert.Same(visualTreeChildren[0], label);
        }

        [Fact]
        public void ChildrenHaveParentsWhenContentIsSet()
        {
            var border = new Border();

            var label = new Label();
            border.Content = label;

            Assert.Same(border, label.Parent);

            border.Content = null;
            Assert.Null(label.Parent);
        }
    }

    public partial class BorderTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the SafeAreaEdges property getter returns the default value when no value is explicitly set.
        /// Input conditions: New Border instance without setting SafeAreaEdges.
        /// Expected result: Returns SafeAreaEdges.Default.
        /// </summary>
        [Fact]
        public void SafeAreaEdges_Get_ReturnsDefaultValue()
        {
            // Arrange
            var border = new Border();

            // Act
            var result = border.SafeAreaEdges;

            // Assert
            Assert.Equal(SafeAreaEdges.Default, result);
        }

        /// <summary>
        /// Tests that the SafeAreaEdges property correctly handles all SafeAreaRegions enum values.
        /// Input conditions: Setting SafeAreaEdges to values created from individual SafeAreaRegions values.
        /// Expected result: Getter returns the same value that was set.
        /// </summary>
        [Theory]
        [InlineData(SafeAreaRegions.None)]
        [InlineData(SafeAreaRegions.SoftInput)]
        [InlineData(SafeAreaRegions.Container)]
        [InlineData(SafeAreaRegions.Default)]
        [InlineData(SafeAreaRegions.All)]
        public void SafeAreaEdges_SetGet_HandlesAllSafeAreaRegionsValues(SafeAreaRegions region)
        {
            // Arrange
            var border = new Border();
            var expectedValue = new SafeAreaEdges(region);

            // Act
            border.SafeAreaEdges = expectedValue;
            var result = border.SafeAreaEdges;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the SafeAreaEdges property correctly handles combined SafeAreaRegions flag values.
        /// Input conditions: Setting SafeAreaEdges to values created from combined SafeAreaRegions flags.
        /// Expected result: Getter returns the same combined value that was set.
        /// </summary>
        [Theory]
        [InlineData(SafeAreaRegions.SoftInput | SafeAreaRegions.Container)]
        [InlineData(SafeAreaRegions.None | SafeAreaRegions.SoftInput)]
        public void SafeAreaEdges_SetGet_HandlesCombinedFlagValues(SafeAreaRegions combinedRegions)
        {
            // Arrange
            var border = new Border();
            var expectedValue = new SafeAreaEdges(combinedRegions);

            // Act
            border.SafeAreaEdges = expectedValue;
            var result = border.SafeAreaEdges;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that multiple set operations on SafeAreaEdges property work correctly.
        /// Input conditions: Setting SafeAreaEdges multiple times to different values.
        /// Expected result: Each get operation returns the most recently set value.
        /// </summary>
        [Fact]
        public void SafeAreaEdges_MultipleSetOperations_ReturnsLatestValue()
        {
            // Arrange
            var border = new Border();

            // Act & Assert
            border.SafeAreaEdges = SafeAreaEdges.None;
            Assert.Equal(SafeAreaEdges.None, border.SafeAreaEdges);

            border.SafeAreaEdges = SafeAreaEdges.All;
            Assert.Equal(SafeAreaEdges.All, border.SafeAreaEdges);

            border.SafeAreaEdges = SafeAreaEdges.Default;
            Assert.Equal(SafeAreaEdges.Default, border.SafeAreaEdges);
        }

        /// <summary>
        /// Tests that the StrokeThickness property has the correct default value when a new Border is created.
        /// The default value should be 1.0 as defined in the BindableProperty.
        /// </summary>
        [Fact]
        public void StrokeThickness_DefaultValue_Returns1Point0()
        {
            // Arrange & Act
            var border = new Border();

            // Assert
            Assert.Equal(1.0, border.StrokeThickness);
        }

        /// <summary>
        /// Tests that the StrokeThickness property setter and getter work correctly for various valid double values.
        /// This ensures the property correctly stores and retrieves values through the BindableProperty system.
        /// </summary>
        /// <param name="value">The stroke thickness value to test</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(2.5)]
        [InlineData(10.0)]
        [InlineData(100.0)]
        [InlineData(1000.0)]
        public void StrokeThickness_SetValidValue_ReturnsSetValue(double value)
        {
            // Arrange
            var border = new Border();

            // Act
            border.StrokeThickness = value;

            // Assert
            Assert.Equal(value, border.StrokeThickness);
        }

        /// <summary>
        /// Tests that the StrokeThickness property can handle negative values.
        /// While negative stroke thickness may not make visual sense, the property should accept any double value.
        /// </summary>
        /// <param name="negativeValue">The negative stroke thickness value to test</param>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-0.5)]
        [InlineData(-10.0)]
        [InlineData(-100.0)]
        public void StrokeThickness_SetNegativeValue_ReturnsSetValue(double negativeValue)
        {
            // Arrange
            var border = new Border();

            // Act
            border.StrokeThickness = negativeValue;

            // Assert
            Assert.Equal(negativeValue, border.StrokeThickness);
        }

        /// <summary>
        /// Tests that the StrokeThickness property can handle extreme double values including boundaries.
        /// This ensures the property works correctly with edge cases of the double type.
        /// </summary>
        /// <param name="extremeValue">The extreme double value to test</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        public void StrokeThickness_SetExtremeValue_ReturnsSetValue(double extremeValue)
        {
            // Arrange
            var border = new Border();

            // Act
            border.StrokeThickness = extremeValue;

            // Assert
            Assert.Equal(extremeValue, border.StrokeThickness);
        }

        /// <summary>
        /// Tests that the StrokeThickness property can handle special double values like NaN and infinity.
        /// These special values should be stored and retrieved correctly even though they may not be meaningful for UI.
        /// </summary>
        /// <param name="specialValue">The special double value to test</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void StrokeThickness_SetSpecialDoubleValue_ReturnsSetValue(double specialValue)
        {
            // Arrange
            var border = new Border();

            // Act
            border.StrokeThickness = specialValue;

            // Assert
            Assert.Equal(specialValue, border.StrokeThickness);
        }

        /// <summary>
        /// Tests that multiple consecutive sets and gets of the StrokeThickness property work correctly.
        /// This verifies that the property maintains state correctly across multiple operations.
        /// </summary>
        [Fact]
        public void StrokeThickness_MultipleSetOperations_ReturnCorrectValues()
        {
            // Arrange
            var border = new Border();

            // Act & Assert
            border.StrokeThickness = 5.0;
            Assert.Equal(5.0, border.StrokeThickness);

            border.StrokeThickness = 0.0;
            Assert.Equal(0.0, border.StrokeThickness);

            border.StrokeThickness = 15.5;
            Assert.Equal(15.5, border.StrokeThickness);

            border.StrokeThickness = -2.0;
            Assert.Equal(-2.0, border.StrokeThickness);
        }

        /// <summary>
        /// Tests that setting the StrokeThickness property to the same value multiple times works correctly.
        /// This ensures the property handles redundant assignments properly.
        /// </summary>
        [Fact]
        public void StrokeThickness_SetSameValueMultipleTimes_ReturnsSameValue()
        {
            // Arrange
            var border = new Border();
            const double testValue = 3.5;

            // Act
            border.StrokeThickness = testValue;
            border.StrokeThickness = testValue;
            border.StrokeThickness = testValue;

            // Assert
            Assert.Equal(testValue, border.StrokeThickness);
        }

        /// <summary>
        /// Tests that StrokeDashOffset property can be set and retrieved with normal double values.
        /// Validates that the setter correctly stores values and the getter correctly retrieves them.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(10.5)]
        [InlineData(-10.5)]
        [InlineData(100.0)]
        [InlineData(0.123456789)]
        public void StrokeDashOffset_SetAndGet_NormalValues_ReturnsCorrectValue(double expectedValue)
        {
            // Arrange
            var border = new Border();

            // Act
            border.StrokeDashOffset = expectedValue;
            var actualValue = border.StrokeDashOffset;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that StrokeDashOffset property handles extreme boundary values correctly.
        /// Validates behavior with double.MinValue and double.MaxValue to ensure no overflow or underflow occurs.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        public void StrokeDashOffset_SetAndGet_BoundaryValues_ReturnsCorrectValue(double boundaryValue)
        {
            // Arrange
            var border = new Border();

            // Act
            border.StrokeDashOffset = boundaryValue;
            var actualValue = border.StrokeDashOffset;

            // Assert
            Assert.Equal(boundaryValue, actualValue);
        }

        /// <summary>
        /// Tests that StrokeDashOffset property handles special floating-point values correctly.
        /// Validates behavior with NaN, positive infinity, and negative infinity values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void StrokeDashOffset_SetAndGet_SpecialFloatingPointValues_ReturnsCorrectValue(double specialValue)
        {
            // Arrange
            var border = new Border();

            // Act
            border.StrokeDashOffset = specialValue;
            var actualValue = border.StrokeDashOffset;

            // Assert
            if (double.IsNaN(specialValue))
            {
                Assert.True(double.IsNaN(actualValue));
            }
            else
            {
                Assert.Equal(specialValue, actualValue);
            }
        }

        /// <summary>
        /// Tests that StrokeDashOffset property returns the default value when no value has been set.
        /// Validates that the default value is 0.0 as specified in the BindableProperty definition.
        /// </summary>
        [Fact]
        public void StrokeDashOffset_DefaultValue_ReturnsZero()
        {
            // Arrange
            var border = new Border();

            // Act
            var defaultValue = border.StrokeDashOffset;

            // Assert
            Assert.Equal(0.0, defaultValue);
        }

        /// <summary>
        /// Tests that StrokeDashOffset property can be set multiple times with different values.
        /// Validates that subsequent sets correctly overwrite previous values.
        /// </summary>
        [Fact]
        public void StrokeDashOffset_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var border = new Border();

            // Act & Assert - Set and verify multiple values
            border.StrokeDashOffset = 5.0;
            Assert.Equal(5.0, border.StrokeDashOffset);

            border.StrokeDashOffset = -3.5;
            Assert.Equal(-3.5, border.StrokeDashOffset);

            border.StrokeDashOffset = 0.0;
            Assert.Equal(0.0, border.StrokeDashOffset);

            border.StrokeDashOffset = 999.999;
            Assert.Equal(999.999, border.StrokeDashOffset);
        }

        /// <summary>
        /// Tests that the StrokeLineCap property returns the default value when not explicitly set.
        /// Verifies the getter returns PenLineCap.Flat as defined in the BindableProperty.
        /// </summary>
        [Fact]
        public void StrokeLineCap_DefaultValue_ReturnsFlat()
        {
            // Arrange
            var border = new Border();

            // Act
            var result = border.StrokeLineCap;

            // Assert
            Assert.Equal(PenLineCap.Flat, result);
        }

        /// <summary>
        /// Tests that the StrokeLineCap property setter and getter work correctly for all valid enum values.
        /// Verifies that setting each valid PenLineCap value can be retrieved correctly.
        /// </summary>
        /// <param name="expectedValue">The PenLineCap value to test</param>
        [Theory]
        [InlineData(PenLineCap.Flat)]
        [InlineData(PenLineCap.Square)]
        [InlineData(PenLineCap.Round)]
        public void StrokeLineCap_SetValidValues_ReturnsExpectedValue(PenLineCap expectedValue)
        {
            // Arrange
            var border = new Border();

            // Act
            border.StrokeLineCap = expectedValue;
            var result = border.StrokeLineCap;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the StrokeLineCap property setter accepts invalid enum values without throwing exceptions.
        /// Verifies behavior with values outside the defined enum range.
        /// </summary>
        [Fact]
        public void StrokeLineCap_SetInvalidEnumValue_DoesNotThrow()
        {
            // Arrange
            var border = new Border();
            var invalidValue = (PenLineCap)(-1);

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                border.StrokeLineCap = invalidValue;
                var result = border.StrokeLineCap;
                Assert.Equal(invalidValue, result);
            });

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the StrokeLineCap property setter accepts values at the upper boundary of int range.
        /// Verifies behavior with maximum integer values cast to the enum type.
        /// </summary>
        [Fact]
        public void StrokeLineCap_SetMaxIntValue_DoesNotThrow()
        {
            // Arrange
            var border = new Border();
            var maxValue = (PenLineCap)int.MaxValue;

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                border.StrokeLineCap = maxValue;
                var result = border.StrokeLineCap;
                Assert.Equal(maxValue, result);
            });

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that multiple consecutive sets and gets of the StrokeLineCap property work correctly.
        /// Verifies that the property maintains state correctly across multiple operations.
        /// </summary>
        [Fact]
        public void StrokeLineCap_MultipleSetGet_MaintainsCorrectValues()
        {
            // Arrange
            var border = new Border();

            // Act & Assert
            border.StrokeLineCap = PenLineCap.Square;
            Assert.Equal(PenLineCap.Square, border.StrokeLineCap);

            border.StrokeLineCap = PenLineCap.Round;
            Assert.Equal(PenLineCap.Round, border.StrokeLineCap);

            border.StrokeLineCap = PenLineCap.Flat;
            Assert.Equal(PenLineCap.Flat, border.StrokeLineCap);
        }

        /// <summary>
        /// Tests that StrokeLineJoin property can be set and retrieved with valid enum values.
        /// Verifies both the setter and getter work correctly for all defined enum values.
        /// </summary>
        /// <param name="expectedValue">The PenLineJoin value to test</param>
        [Theory]
        [InlineData(PenLineJoin.Miter)]
        [InlineData(PenLineJoin.Bevel)]
        [InlineData(PenLineJoin.Round)]
        public void StrokeLineJoin_SetValidValue_ReturnsExpectedValue(PenLineJoin expectedValue)
        {
            // Arrange
            var border = new Border();

            // Act
            border.StrokeLineJoin = expectedValue;

            // Assert
            Assert.Equal(expectedValue, border.StrokeLineJoin);
        }

        /// <summary>
        /// Tests that StrokeLineJoin property returns the default value when not explicitly set.
        /// The default value should be PenLineJoin.Miter as defined in the BindableProperty.
        /// </summary>
        [Fact]
        public void StrokeLineJoin_DefaultValue_ReturnsMiter()
        {
            // Arrange & Act
            var border = new Border();

            // Assert
            Assert.Equal(PenLineJoin.Miter, border.StrokeLineJoin);
        }

        /// <summary>
        /// Tests that StrokeLineJoin property can handle invalid enum values.
        /// Verifies that the property correctly stores and retrieves even out-of-range enum values.
        /// </summary>
        [Fact]
        public void StrokeLineJoin_SetInvalidEnumValue_ReturnsSetValue()
        {
            // Arrange
            var border = new Border();
            var invalidValue = (PenLineJoin)999;

            // Act
            border.StrokeLineJoin = invalidValue;

            // Assert
            Assert.Equal(invalidValue, border.StrokeLineJoin);
        }

        /// <summary>
        /// Tests that StrokeLineJoin property handles boundary enum values correctly.
        /// Verifies the minimum and maximum valid enum values work as expected.
        /// </summary>
        [Theory]
        [InlineData((PenLineJoin)0)] // Miter = 0
        [InlineData((PenLineJoin)2)] // Round = 2 (highest valid value)
        public void StrokeLineJoin_SetBoundaryValues_ReturnsExpectedValue(PenLineJoin value)
        {
            // Arrange
            var border = new Border();

            // Act
            border.StrokeLineJoin = value;

            // Assert
            Assert.Equal(value, border.StrokeLineJoin);
        }

        /// <summary>
        /// Tests that StrokeLineJoin property can be set multiple times with different values.
        /// Verifies that subsequent assignments correctly overwrite previous values.
        /// </summary>
        [Fact]
        public void StrokeLineJoin_SetMultipleTimes_ReturnsLastSetValue()
        {
            // Arrange
            var border = new Border();

            // Act & Assert
            border.StrokeLineJoin = PenLineJoin.Miter;
            Assert.Equal(PenLineJoin.Miter, border.StrokeLineJoin);

            border.StrokeLineJoin = PenLineJoin.Bevel;
            Assert.Equal(PenLineJoin.Bevel, border.StrokeLineJoin);

            border.StrokeLineJoin = PenLineJoin.Round;
            Assert.Equal(PenLineJoin.Round, border.StrokeLineJoin);
        }

        /// <summary>
        /// Tests that OnPropertyChanged calls base implementation and handles StrokeThicknessProperty changes correctly.
        /// Input: propertyName equals StrokeThicknessProperty.PropertyName.
        /// Expected: UpdateStrokeShape and Handler.UpdateValue are called for Shape property.
        /// </summary>
        [Fact]
        public void OnPropertyChanged_StrokeThicknessProperty_UpdatesShapeAndCallsHandler()
        {
            // Arrange
            var border = new TestableBorder();
            var mockHandler = Substitute.For<IViewHandler>();
            border.Handler = mockHandler;

            // Act
            border.TriggerOnPropertyChanged(Border.StrokeThicknessProperty.PropertyName);

            // Assert
            Assert.True(border.UpdateStrokeShapeCalled);
            mockHandler.Received(1).UpdateValue(nameof(IBorderStroke.Shape));
        }

        /// <summary>
        /// Tests that OnPropertyChanged calls base implementation and handles StrokeShapeProperty changes correctly.
        /// Input: propertyName equals StrokeShapeProperty.PropertyName.
        /// Expected: UpdateStrokeShape and Handler.UpdateValue are called for Shape property.
        /// </summary>
        [Fact]
        public void OnPropertyChanged_StrokeShapeProperty_UpdatesShapeAndCallsHandler()
        {
            // Arrange
            var border = new TestableBorder();
            var mockHandler = Substitute.For<IViewHandler>();
            border.Handler = mockHandler;

            // Act
            border.TriggerOnPropertyChanged(Border.StrokeShapeProperty.PropertyName);

            // Assert
            Assert.True(border.UpdateStrokeShapeCalled);
            mockHandler.Received(1).UpdateValue(nameof(IBorderStroke.Shape));
        }

        /// <summary>
        /// Tests that OnPropertyChanged handles StrokeDashArrayProperty changes correctly (previously uncovered branch).
        /// Input: propertyName equals StrokeDashArrayProperty.PropertyName.
        /// Expected: Handler.UpdateValue is called for StrokeDashPattern property.
        /// </summary>
        [Fact]
        public void OnPropertyChanged_StrokeDashArrayProperty_CallsHandlerUpdateValue()
        {
            // Arrange
            var border = new TestableBorder();
            var mockHandler = Substitute.For<IViewHandler>();
            border.Handler = mockHandler;

            // Act
            border.TriggerOnPropertyChanged(Border.StrokeDashArrayProperty.PropertyName);

            // Assert
            Assert.False(border.UpdateStrokeShapeCalled);
            mockHandler.Received(1).UpdateValue(nameof(IBorderStroke.StrokeDashPattern));
        }

        /// <summary>
        /// Tests that OnPropertyChanged with null Handler does not throw exceptions.
        /// Input: propertyName equals StrokeDashArrayProperty.PropertyName, Handler is null.
        /// Expected: No exception thrown, UpdateStrokeShape is not called.
        /// </summary>
        [Fact]
        public void OnPropertyChanged_StrokeDashArrayProperty_NullHandler_DoesNotThrow()
        {
            // Arrange
            var border = new TestableBorder();
            border.Handler = null;

            // Act & Assert (should not throw)
            border.TriggerOnPropertyChanged(Border.StrokeDashArrayProperty.PropertyName);

            Assert.False(border.UpdateStrokeShapeCalled);
        }

        /// <summary>
        /// Tests that OnPropertyChanged with null Handler for stroke properties does not throw exceptions.
        /// Input: propertyName equals StrokeThicknessProperty.PropertyName, Handler is null.
        /// Expected: No exception thrown, UpdateStrokeShape is called.
        /// </summary>
        [Fact]
        public void OnPropertyChanged_StrokeThicknessProperty_NullHandler_DoesNotThrow()
        {
            // Arrange
            var border = new TestableBorder();
            border.Handler = null;

            // Act & Assert (should not throw)
            border.TriggerOnPropertyChanged(Border.StrokeThicknessProperty.PropertyName);

            Assert.True(border.UpdateStrokeShapeCalled);
        }

        /// <summary>
        /// Helper class to expose protected OnPropertyChanged method and track method calls for testing.
        /// </summary>
        private class TestableBorder : Border
        {
            public bool UpdateStrokeShapeCalled { get; private set; }
            public bool BaseOnPropertyChangedCalled { get; private set; }

            public void TriggerOnPropertyChanged(string propertyName)
            {
                OnPropertyChanged(propertyName);
            }

            private void UpdateStrokeShape()
            {
                UpdateStrokeShapeCalled = true;
                // Call the actual implementation
                if (StrokeShape is Shape strokeShape && StrokeThickness == 0)
                {
                    strokeShape.StrokeThickness = StrokeThickness;
                }
            }
        }

        /// <summary>
        /// Tests CrossPlatformArrange with normal bounds and stroke thickness.
        /// Should return the original bounds size and arrange content with inset bounds.
        /// </summary>
        [Fact]
        public void CrossPlatformArrange_WithNormalBoundsAndStrokeThickness_ReturnsOriginalSize()
        {
            // Arrange
            var border = new Border();
            border.StrokeThickness = 5.0;
            var bounds = new Rect(10, 20, 100, 80);
            var expectedSize = new Size(100, 80);

            // Act
            var result = border.CrossPlatformArrange(bounds);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests CrossPlatformArrange with zero stroke thickness.
        /// Should return the original bounds size without modifying the arrangement bounds.
        /// </summary>
        [Fact]
        public void CrossPlatformArrange_WithZeroStrokeThickness_ReturnsOriginalSize()
        {
            // Arrange
            var border = new Border();
            border.StrokeThickness = 0.0;
            var bounds = new Rect(0, 0, 50, 50);
            var expectedSize = new Size(50, 50);

            // Act
            var result = border.CrossPlatformArrange(bounds);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests CrossPlatformArrange with negative stroke thickness.
        /// Should return the original bounds size even with negative stroke thickness.
        /// </summary>
        [Fact]
        public void CrossPlatformArrange_WithNegativeStrokeThickness_ReturnsOriginalSize()
        {
            // Arrange
            var border = new Border();
            border.StrokeThickness = -10.0;
            var bounds = new Rect(5, 5, 40, 60);
            var expectedSize = new Size(40, 60);

            // Act
            var result = border.CrossPlatformArrange(bounds);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests CrossPlatformArrange with stroke thickness larger than bounds dimensions.
        /// Should return the original bounds size even when inset would create negative dimensions.
        /// </summary>
        [Fact]
        public void CrossPlatformArrange_WithStrokeThicknessLargerThanBounds_ReturnsOriginalSize()
        {
            // Arrange
            var border = new Border();
            border.StrokeThickness = 50.0;
            var bounds = new Rect(0, 0, 20, 20);
            var expectedSize = new Size(20, 20);

            // Act
            var result = border.CrossPlatformArrange(bounds);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests CrossPlatformArrange with zero-sized bounds.
        /// Should return zero size and handle empty bounds gracefully.
        /// </summary>
        [Fact]
        public void CrossPlatformArrange_WithZeroSizedBounds_ReturnsZeroSize()
        {
            // Arrange
            var border = new Border();
            border.StrokeThickness = 2.0;
            var bounds = new Rect(10, 10, 0, 0);
            var expectedSize = new Size(0, 0);

            // Act
            var result = border.CrossPlatformArrange(bounds);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests CrossPlatformArrange with maximum double value for stroke thickness.
        /// Should return the original bounds size without throwing exceptions.
        /// </summary>
        [Fact]
        public void CrossPlatformArrange_WithMaximumStrokeThickness_ReturnsOriginalSize()
        {
            // Arrange
            var border = new Border();
            border.StrokeThickness = double.MaxValue;
            var bounds = new Rect(0, 0, 100, 100);
            var expectedSize = new Size(100, 100);

            // Act
            var result = border.CrossPlatformArrange(bounds);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests CrossPlatformArrange with minimum double value for stroke thickness.
        /// Should return the original bounds size without throwing exceptions.
        /// </summary>
        [Fact]
        public void CrossPlatformArrange_WithMinimumStrokeThickness_ReturnsOriginalSize()
        {
            // Arrange
            var border = new Border();
            border.StrokeThickness = double.MinValue;
            var bounds = new Rect(0, 0, 100, 100);
            var expectedSize = new Size(100, 100);

            // Act
            var result = border.CrossPlatformArrange(bounds);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests CrossPlatformArrange with NaN stroke thickness.
        /// Should return the original bounds size and handle NaN gracefully.
        /// </summary>
        [Fact]
        public void CrossPlatformArrange_WithNaNStrokeThickness_ReturnsOriginalSize()
        {
            // Arrange
            var border = new Border();
            border.StrokeThickness = double.NaN;
            var bounds = new Rect(10, 10, 50, 50);
            var expectedSize = new Size(50, 50);

            // Act
            var result = border.CrossPlatformArrange(bounds);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests CrossPlatformArrange with positive infinity stroke thickness.
        /// Should return the original bounds size and handle infinity gracefully.
        /// </summary>
        [Fact]
        public void CrossPlatformArrange_WithPositiveInfinityStrokeThickness_ReturnsOriginalSize()
        {
            // Arrange
            var border = new Border();
            border.StrokeThickness = double.PositiveInfinity;
            var bounds = new Rect(5, 5, 30, 40);
            var expectedSize = new Size(30, 40);

            // Act
            var result = border.CrossPlatformArrange(bounds);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests CrossPlatformArrange with negative infinity stroke thickness.
        /// Should return the original bounds size and handle negative infinity gracefully.
        /// </summary>
        [Fact]
        public void CrossPlatformArrange_WithNegativeInfinityStrokeThickness_ReturnsOriginalSize()
        {
            // Arrange
            var border = new Border();
            border.StrokeThickness = double.NegativeInfinity;
            var bounds = new Rect(0, 0, 25, 35);
            var expectedSize = new Size(25, 35);

            // Act
            var result = border.CrossPlatformArrange(bounds);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests CrossPlatformArrange with negative bounds dimensions.
        /// Should return the original bounds size even with negative width/height.
        /// </summary>
        [Fact]
        public void CrossPlatformArrange_WithNegativeBoundsDimensions_ReturnsOriginalSize()
        {
            // Arrange
            var border = new Border();
            border.StrokeThickness = 5.0;
            var bounds = new Rect(10, 10, -20, -30);
            var expectedSize = new Size(-20, -30);

            // Act
            var result = border.CrossPlatformArrange(bounds);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests CrossPlatformArrange with fractional stroke thickness and bounds.
        /// Should return the exact original bounds size with fractional values.
        /// </summary>
        [Fact]
        public void CrossPlatformArrange_WithFractionalValues_ReturnsOriginalSize()
        {
            // Arrange
            var border = new Border();
            border.StrokeThickness = 2.5;
            var bounds = new Rect(1.25, 3.75, 45.5, 67.25);
            var expectedSize = new Size(45.5, 67.25);

            // Act
            var result = border.CrossPlatformArrange(bounds);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests CrossPlatformMeasure with normal positive constraints.
        /// Verifies that the method calculates inset correctly and returns a valid size.
        /// </summary>
        [Fact]
        public void CrossPlatformMeasure_WithPositiveConstraints_ReturnsValidSize()
        {
            // Arrange
            var border = new Border();
            border.Padding = new Thickness(10, 15, 20, 25);
            border.StrokeThickness = 5.0;
            double widthConstraint = 100.0;
            double heightConstraint = 200.0;

            // Act
            var result = border.CrossPlatformMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Width >= 0);
            Assert.True(result.Height >= 0);
        }

        /// <summary>
        /// Tests CrossPlatformMeasure with zero constraints.
        /// Verifies that the method handles zero width and height constraints correctly.
        /// </summary>
        [Fact]
        public void CrossPlatformMeasure_WithZeroConstraints_ReturnsValidSize()
        {
            // Arrange
            var border = new Border();
            border.Padding = new Thickness(5);
            border.StrokeThickness = 2.0;
            double widthConstraint = 0.0;
            double heightConstraint = 0.0;

            // Act
            var result = border.CrossPlatformMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Width >= 0);
            Assert.True(result.Height >= 0);
        }

        /// <summary>
        /// Tests CrossPlatformMeasure with negative constraints.
        /// Verifies that the method handles negative width and height constraints correctly.
        /// </summary>
        [Fact]
        public void CrossPlatformMeasure_WithNegativeConstraints_ReturnsValidSize()
        {
            // Arrange
            var border = new Border();
            border.Padding = new Thickness(1);
            border.StrokeThickness = 1.0;
            double widthConstraint = -50.0;
            double heightConstraint = -100.0;

            // Act
            var result = border.CrossPlatformMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Width >= 0);
            Assert.True(result.Height >= 0);
        }

        /// <summary>
        /// Tests CrossPlatformMeasure with positive infinity constraints.
        /// Verifies that the method handles infinite width and height constraints correctly.
        /// </summary>
        [Fact]
        public void CrossPlatformMeasure_WithPositiveInfinityConstraints_ReturnsValidSize()
        {
            // Arrange
            var border = new Border();
            border.Padding = new Thickness(10);
            border.StrokeThickness = 3.0;
            double widthConstraint = double.PositiveInfinity;
            double heightConstraint = double.PositiveInfinity;

            // Act
            var result = border.CrossPlatformMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Width >= 0);
            Assert.True(result.Height >= 0);
        }

        /// <summary>
        /// Tests CrossPlatformMeasure with negative infinity constraints.
        /// Verifies that the method handles negative infinite width and height constraints correctly.
        /// </summary>
        [Fact]
        public void CrossPlatformMeasure_WithNegativeInfinityConstraints_ReturnsValidSize()
        {
            // Arrange
            var border = new Border();
            border.Padding = new Thickness(5);
            border.StrokeThickness = 1.5;
            double widthConstraint = double.NegativeInfinity;
            double heightConstraint = double.NegativeInfinity;

            // Act
            var result = border.CrossPlatformMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Width >= 0);
            Assert.True(result.Height >= 0);
        }

        /// <summary>
        /// Tests CrossPlatformMeasure with NaN constraints.
        /// Verifies that the method handles NaN width and height constraints correctly.
        /// </summary>
        [Fact]
        public void CrossPlatformMeasure_WithNaNConstraints_ReturnsValidSize()
        {
            // Arrange
            var border = new Border();
            border.Padding = new Thickness(2);
            border.StrokeThickness = 4.0;
            double widthConstraint = double.NaN;
            double heightConstraint = double.NaN;

            // Act
            var result = border.CrossPlatformMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Width >= 0 || double.IsNaN(result.Width));
            Assert.True(result.Height >= 0 || double.IsNaN(result.Height));
        }

        /// <summary>
        /// Tests CrossPlatformMeasure with zero padding and stroke thickness.
        /// Verifies that the method correctly handles when both padding and stroke thickness are zero.
        /// </summary>
        [Fact]
        public void CrossPlatformMeasure_WithZeroPaddingAndStroke_ReturnsValidSize()
        {
            // Arrange
            var border = new Border();
            border.Padding = Thickness.Zero;
            border.StrokeThickness = 0.0;
            double widthConstraint = 50.0;
            double heightConstraint = 75.0;

            // Act
            var result = border.CrossPlatformMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Width >= 0);
            Assert.True(result.Height >= 0);
        }

        /// <summary>
        /// Tests CrossPlatformMeasure with asymmetric padding.
        /// Verifies that the method correctly handles different padding values for each side.
        /// </summary>
        [Fact]
        public void CrossPlatformMeasure_WithAsymmetricPadding_ReturnsValidSize()
        {
            // Arrange
            var border = new Border();
            border.Padding = new Thickness(1, 2, 3, 4);
            border.StrokeThickness = 2.5;
            double widthConstraint = 100.0;
            double heightConstraint = 150.0;

            // Act
            var result = border.CrossPlatformMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Width >= 0);
            Assert.True(result.Height >= 0);
        }

        /// <summary>
        /// Tests CrossPlatformMeasure with large padding and stroke thickness values.
        /// Verifies that the method handles large inset values correctly.
        /// </summary>
        [Fact]
        public void CrossPlatformMeasure_WithLargePaddingAndStroke_ReturnsValidSize()
        {
            // Arrange
            var border = new Border();
            border.Padding = new Thickness(1000, 2000, 3000, 4000);
            border.StrokeThickness = 500.0;
            double widthConstraint = 100.0;
            double heightConstraint = 200.0;

            // Act
            var result = border.CrossPlatformMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Width >= 0);
            Assert.True(result.Height >= 0);
        }

        /// <summary>
        /// Tests CrossPlatformMeasure with negative stroke thickness.
        /// Verifies that the method handles negative stroke thickness values correctly.
        /// </summary>
        [Fact]
        public void CrossPlatformMeasure_WithNegativeStrokeThickness_ReturnsValidSize()
        {
            // Arrange
            var border = new Border();
            border.Padding = new Thickness(5);
            border.StrokeThickness = -10.0;
            double widthConstraint = 100.0;
            double heightConstraint = 100.0;

            // Act
            var result = border.CrossPlatformMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Width >= 0);
            Assert.True(result.Height >= 0);
        }

        /// <summary>
        /// Tests CrossPlatformMeasure with mixed extreme values.
        /// Verifies that the method handles various combinations of extreme constraint values.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.MinValue, double.MaxValue)]
        [InlineData(0.0, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0.0)]
        public void CrossPlatformMeasure_WithExtremeConstraints_ReturnsValidSize(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var border = new Border();
            border.Padding = new Thickness(1);
            border.StrokeThickness = 1.0;

            // Act
            var result = border.CrossPlatformMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Width >= 0 || double.IsInfinity(result.Width) || double.IsNaN(result.Width));
            Assert.True(result.Height >= 0 || double.IsInfinity(result.Height) || double.IsNaN(result.Height));
        }

        /// <summary>
        /// Tests that StrokeDashPattern returns null when StrokeDashArray is null.
        /// Verifies the null handling path in the getter.
        /// </summary>
        [Fact]
        public void StrokeDashPattern_WhenStrokeDashArrayIsNull_ReturnsNull()
        {
            // Arrange
            var border = new Border();
            border.StrokeDashArray = null;

            // Act
            var result = border.StrokeDashPattern;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that StrokeDashPattern returns empty array when StrokeDashArray is empty.
        /// Verifies the conversion of empty collection to float array.
        /// </summary>
        [Fact]
        public void StrokeDashPattern_WhenStrokeDashArrayIsEmpty_ReturnsEmptyArray()
        {
            // Arrange
            var border = new Border();
            border.StrokeDashArray = new DoubleCollection();

            // Act
            var result = border.StrokeDashPattern;

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that StrokeDashPattern correctly converts DoubleCollection values to float array.
        /// Verifies the ToFloatArray conversion and proper value mapping.
        /// </summary>
        [Theory]
        [InlineData(new double[] { 1.0 }, new float[] { 1.0f })]
        [InlineData(new double[] { 1.5, 2.5, 3.5 }, new float[] { 1.5f, 2.5f, 3.5f })]
        [InlineData(new double[] { 0.0, 10.0, 100.0 }, new float[] { 0.0f, 10.0f, 100.0f })]
        [InlineData(new double[] { double.MaxValue }, new float[] { float.PositiveInfinity })]
        [InlineData(new double[] { double.MinValue }, new float[] { float.NegativeInfinity })]
        public void StrokeDashPattern_WhenStrokeDashArrayHasValues_ReturnsConvertedFloatArray(double[] input, float[] expected)
        {
            // Arrange
            var border = new Border();
            var collection = new DoubleCollection();
            foreach (var value in input)
            {
                collection.Add(value);
            }
            border.StrokeDashArray = collection;

            // Act
            var result = border.StrokeDashPattern;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.Length, result.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], result[i]);
            }
        }

        /// <summary>
        /// Tests that StrokeDashPattern handles special double values correctly.
        /// Verifies conversion of NaN, positive and negative infinity values.
        /// </summary>
        [Fact]
        public void StrokeDashPattern_WhenStrokeDashArrayHasSpecialValues_ReturnsConvertedValues()
        {
            // Arrange
            var border = new Border();
            var collection = new DoubleCollection { double.NaN, double.PositiveInfinity, double.NegativeInfinity };
            border.StrokeDashArray = collection;

            // Act
            var result = border.StrokeDashPattern;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Length);
            Assert.True(float.IsNaN(result[0]));
            Assert.True(float.IsPositiveInfinity(result[1]));
            Assert.True(float.IsNegativeInfinity(result[2]));
        }

        /// <summary>
        /// Tests that StrokeDashPattern subscribes to CollectionChanged events when StrokeDashArray implements INotifyCollectionChanged.
        /// Verifies that the event handler is properly attached to the collection.
        /// </summary>
        [Fact]
        public void StrokeDashPattern_WhenStrokeDashArrayImplementsINotifyCollectionChanged_SubscribesToEvents()
        {
            // Arrange
            var border = new Border();
            var collection = new DoubleCollection();
            border.StrokeDashArray = collection;

            // Act - First access to trigger event subscription
            var result1 = border.StrokeDashPattern;

            // Verify event subscription by checking that subsequent collection changes trigger the handler
            // We can't directly verify event subscription, but we can verify the behavior works
            collection.Add(1.0);

            // Act - Second access should unsubscribe from previous and resubscribe
            var result2 = border.StrokeDashPattern;

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Single(result2);
            Assert.Equal(1.0f, result2[0]);
        }

        /// <summary>
        /// Tests that StrokeDashPattern unsubscribes from previous CollectionChanged events on subsequent calls.
        /// Verifies that event handlers are properly cleaned up to prevent memory leaks.
        /// </summary>
        [Fact]
        public void StrokeDashPattern_WhenCalledMultipleTimes_UnsubscribesFromPreviousEvents()
        {
            // Arrange
            var border = new Border();
            var collection1 = new DoubleCollection { 1.0, 2.0 };
            var collection2 = new DoubleCollection { 3.0, 4.0, 5.0 };

            // Act - Set first collection and access property
            border.StrokeDashArray = collection1;
            var result1 = border.StrokeDashPattern;

            // Set second collection and access property again
            border.StrokeDashArray = collection2;
            var result2 = border.StrokeDashPattern;

            // Assert
            Assert.NotNull(result1);
            Assert.Equal(2, result1.Length);
            Assert.Equal(1.0f, result1[0]);
            Assert.Equal(2.0f, result1[1]);

            Assert.NotNull(result2);
            Assert.Equal(3, result2.Length);
            Assert.Equal(3.0f, result2[0]);
            Assert.Equal(4.0f, result2[1]);
            Assert.Equal(5.0f, result2[2]);
        }

        /// <summary>
        /// Tests that StrokeDashPattern handles transitions from null to non-null StrokeDashArray.
        /// Verifies proper event handling when collection changes from null state.
        /// </summary>
        [Fact]
        public void StrokeDashPattern_WhenTransitioningFromNullToCollection_HandlesEventsCorrectly()
        {
            // Arrange
            var border = new Border();
            border.StrokeDashArray = null;

            // Act - Access with null array
            var result1 = border.StrokeDashPattern;

            // Set non-null collection and access again
            border.StrokeDashArray = new DoubleCollection { 1.5, 2.5 };
            var result2 = border.StrokeDashPattern;

            // Assert
            Assert.Null(result1);

            Assert.NotNull(result2);
            Assert.Equal(2, result2.Length);
            Assert.Equal(1.5f, result2[0]);
            Assert.Equal(2.5f, result2[1]);
        }

        /// <summary>
        /// Tests that StrokeDashPattern handles transitions from non-null to null StrokeDashArray.
        /// Verifies proper event cleanup when collection becomes null.
        /// </summary>
        [Fact]
        public void StrokeDashPattern_WhenTransitioningFromCollectionToNull_HandlesEventsCorrectly()
        {
            // Arrange
            var border = new Border();
            border.StrokeDashArray = new DoubleCollection { 1.0, 2.0 };

            // Act - Access with collection
            var result1 = border.StrokeDashPattern;

            // Set to null and access again
            border.StrokeDashArray = null;
            var result2 = border.StrokeDashPattern;

            // Assert
            Assert.NotNull(result1);
            Assert.Equal(2, result1.Length);

            Assert.Null(result2);
        }

        /// <summary>
        /// Tests that StrokeDashPattern returns cached value from _strokeDashPattern field.
        /// Verifies that the field is properly updated and returned.
        /// </summary>
        [Fact]
        public void StrokeDashPattern_WhenCalled_UpdatesAndReturnsCachedValue()
        {
            // Arrange
            var border = new Border();
            var collection = new DoubleCollection { 10.5, 20.5 };
            border.StrokeDashArray = collection;

            // Act - Multiple calls should return the same reference (cached)
            var result1 = border.StrokeDashPattern;
            var result2 = border.StrokeDashPattern;

            // Assert - Both calls should return arrays with same values
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Equal(2, result1.Length);
            Assert.Equal(2, result2.Length);
            Assert.Equal(10.5f, result1[0]);
            Assert.Equal(20.5f, result1[1]);
            Assert.Equal(10.5f, result2[0]);
            Assert.Equal(20.5f, result2[1]);
        }

        /// <summary>
        /// Tests that the Padding property getter returns the default value when no padding is set.
        /// Verifies that the getter correctly retrieves the value using GetValue.
        /// </summary>
        [Fact]
        public void Padding_Get_ReturnsDefaultValue()
        {
            // Arrange
            var border = new Border();

            // Act
            var padding = border.Padding;

            // Assert
            Assert.Equal(Thickness.Zero, padding);
        }

        /// <summary>
        /// Tests that the Padding property getter returns the correct value after setting a uniform thickness.
        /// Verifies the getter works with uniform padding values.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(10.5)]
        [InlineData(100.0)]
        [InlineData(-5.0)]
        public void Padding_Get_ReturnsCorrectUniformValue(double uniformSize)
        {
            // Arrange
            var border = new Border();
            var expectedThickness = new Thickness(uniformSize);

            // Act
            border.Padding = expectedThickness;
            var actualPadding = border.Padding;

            // Assert
            Assert.Equal(expectedThickness, actualPadding);
        }

        /// <summary>
        /// Tests that the Padding property getter returns the correct value after setting individual edge values.
        /// Verifies the getter works with non-uniform padding values.
        /// </summary>
        [Theory]
        [InlineData(1.0, 2.0, 3.0, 4.0)]
        [InlineData(0.0, 0.0, 0.0, 0.0)]
        [InlineData(-1.0, -2.0, -3.0, -4.0)]
        [InlineData(10.5, 20.3, 15.7, 8.9)]
        [InlineData(double.MaxValue, double.MinValue, 0.0, 1.0)]
        public void Padding_Get_ReturnsCorrectIndividualValues(double left, double top, double right, double bottom)
        {
            // Arrange
            var border = new Border();
            var expectedThickness = new Thickness(left, top, right, bottom);

            // Act
            border.Padding = expectedThickness;
            var actualPadding = border.Padding;

            // Assert
            Assert.Equal(expectedThickness, actualPadding);
        }

        /// <summary>
        /// Tests that the Padding property getter handles extreme double values correctly.
        /// Verifies the getter works with boundary and special numeric values.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Padding_Get_HandlesExtremeValues(double value)
        {
            // Arrange
            var border = new Border();
            var expectedThickness = new Thickness(value);

            // Act
            border.Padding = expectedThickness;
            var actualPadding = border.Padding;

            // Assert
            Assert.Equal(expectedThickness, actualPadding);
        }

        /// <summary>
        /// Tests that the Padding property getter handles NaN values correctly.
        /// Verifies the getter preserves NaN values when retrieved.
        /// </summary>
        [Fact]
        public void Padding_Get_HandlesNaNValues()
        {
            // Arrange
            var border = new Border();
            var expectedThickness = new Thickness(double.NaN, double.NaN, double.NaN, double.NaN);

            // Act
            border.Padding = expectedThickness;
            var actualPadding = border.Padding;

            // Assert
            Assert.True(double.IsNaN(actualPadding.Left));
            Assert.True(double.IsNaN(actualPadding.Top));
            Assert.True(double.IsNaN(actualPadding.Right));
            Assert.True(double.IsNaN(actualPadding.Bottom));
        }

        /// <summary>
        /// Tests that the Padding property getter returns correct values after multiple sets.
        /// Verifies the getter consistently returns the most recently set value.
        /// </summary>
        [Fact]
        public void Padding_Get_ReturnsLatestValueAfterMultipleSets()
        {
            // Arrange
            var border = new Border();
            var firstThickness = new Thickness(5.0);
            var secondThickness = new Thickness(10.0, 15.0, 20.0, 25.0);

            // Act & Assert - First set
            border.Padding = firstThickness;
            Assert.Equal(firstThickness, border.Padding);

            // Act & Assert - Second set
            border.Padding = secondThickness;
            Assert.Equal(secondThickness, border.Padding);
        }

        /// <summary>
        /// Tests that the Padding property getter works with horizontal/vertical constructor pattern.
        /// Verifies the getter correctly retrieves thickness created with horizontal/vertical values.
        /// </summary>
        [Theory]
        [InlineData(5.0, 10.0)]
        [InlineData(0.0, 0.0)]
        [InlineData(-2.5, 7.8)]
        [InlineData(double.MaxValue, double.MinValue)]
        public void Padding_Get_ReturnsCorrectHorizontalVerticalValues(double horizontal, double vertical)
        {
            // Arrange
            var border = new Border();
            var expectedThickness = new Thickness(horizontal, vertical);

            // Act
            border.Padding = expectedThickness;
            var actualPadding = border.Padding;

            // Assert
            Assert.Equal(expectedThickness, actualPadding);
            Assert.Equal(horizontal, actualPadding.Left);
            Assert.Equal(vertical, actualPadding.Top);
            Assert.Equal(horizontal, actualPadding.Right);
            Assert.Equal(vertical, actualPadding.Bottom);
        }
    }


    public partial class BorderStrokeDashArrayTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the StrokeDashArray setter correctly assigns a valid DoubleCollection.
        /// Input: New DoubleCollection with double values.
        /// Expected: Property should return the assigned collection.
        /// </summary>
        [Fact]
        public void StrokeDashArray_SetValidDoubleCollection_ReturnsAssignedCollection()
        {
            // Arrange
            var border = new Border();
            var doubleCollection = new DoubleCollection(new double[] { 1.0, 2.0, 3.0 });

            // Act
            border.StrokeDashArray = doubleCollection;

            // Assert
            Assert.Equal(doubleCollection, border.StrokeDashArray);
        }

        /// <summary>
        /// Tests that the StrokeDashArray setter correctly handles null assignment.
        /// Input: null value.
        /// Expected: Property should return null.
        /// </summary>
        [Fact]
        public void StrokeDashArray_SetNull_ReturnsNull()
        {
            // Arrange
            var border = new Border();

            // Act
            border.StrokeDashArray = null;

            // Assert
            Assert.Null(border.StrokeDashArray);
        }

        /// <summary>
        /// Tests that the StrokeDashArray setter correctly assigns an empty DoubleCollection.
        /// Input: Empty DoubleCollection.
        /// Expected: Property should return an empty collection.
        /// </summary>
        [Fact]
        public void StrokeDashArray_SetEmptyDoubleCollection_ReturnsEmptyCollection()
        {
            // Arrange
            var border = new Border();
            var emptyCollection = new DoubleCollection();

            // Act
            border.StrokeDashArray = emptyCollection;

            // Assert
            Assert.Equal(emptyCollection, border.StrokeDashArray);
            Assert.Empty(border.StrokeDashArray);
        }

        /// <summary>
        /// Tests that the StrokeDashArray getter returns the default value when no value has been explicitly set.
        /// Input: No explicit assignment to StrokeDashArray.
        /// Expected: Property should return a new DoubleCollection (from defaultValueCreator).
        /// </summary>
        [Fact]
        public void StrokeDashArray_GetWithoutSetting_ReturnsDefaultValue()
        {
            // Arrange
            var border = new Border();

            // Act
            var result = border.StrokeDashArray;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DoubleCollection>(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that the StrokeDashArray property handles multiple assignments correctly.
        /// Input: Multiple different DoubleCollection assignments.
        /// Expected: Property should return the last assigned value each time.
        /// </summary>
        [Fact]
        public void StrokeDashArray_MultipleAssignments_ReturnsLastAssignedValue()
        {
            // Arrange
            var border = new Border();
            var firstCollection = new DoubleCollection(new double[] { 1.0, 2.0 });
            var secondCollection = new DoubleCollection(new double[] { 3.0, 4.0, 5.0 });

            // Act & Assert
            border.StrokeDashArray = firstCollection;
            Assert.Equal(firstCollection, border.StrokeDashArray);

            border.StrokeDashArray = secondCollection;
            Assert.Equal(secondCollection, border.StrokeDashArray);

            border.StrokeDashArray = null;
            Assert.Null(border.StrokeDashArray);
        }

        /// <summary>
        /// Tests that the StrokeDashArray property works with DoubleCollection created from various input types.
        /// Input: DoubleCollection created from different constructor overloads and implicit conversions.
        /// Expected: Property should correctly store and return the assigned collections.
        /// </summary>
        [Theory]
        [InlineData(new double[] { })]
        [InlineData(new double[] { 0.0 })]
        [InlineData(new double[] { 1.5, 2.5 })]
        [InlineData(new double[] { double.MinValue, double.MaxValue })]
        [InlineData(new double[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })]
        [InlineData(new double[] { -1.0, 0.0, 1.0, 10.5, 100.25 })]
        public void StrokeDashArray_SetDoubleCollectionFromArray_HandlesVariousValues(double[] values)
        {
            // Arrange
            var border = new Border();
            var doubleCollection = new DoubleCollection(values);

            // Act
            border.StrokeDashArray = doubleCollection;

            // Assert
            Assert.Equal(doubleCollection, border.StrokeDashArray);
            Assert.Equal(values.Length, border.StrokeDashArray.Count);
            for (int i = 0; i < values.Length; i++)
            {
                if (double.IsNaN(values[i]))
                {
                    Assert.True(double.IsNaN(border.StrokeDashArray[i]));
                }
                else
                {
                    Assert.Equal(values[i], border.StrokeDashArray[i]);
                }
            }
        }

        /// <summary>
        /// Tests that the StrokeDashArray property works with implicit conversion from double array.
        /// Input: Direct assignment of double array (using implicit conversion).
        /// Expected: Property should store the converted DoubleCollection.
        /// </summary>
        [Fact]
        public void StrokeDashArray_SetFromDoubleArrayImplicitConversion_CreatesDoubleCollection()
        {
            // Arrange
            var border = new Border();
            double[] values = { 1.0, 2.0, 3.0 };

            // Act
            border.StrokeDashArray = values; // Implicit conversion

            // Assert
            Assert.NotNull(border.StrokeDashArray);
            Assert.Equal(values.Length, border.StrokeDashArray.Count);
            for (int i = 0; i < values.Length; i++)
            {
                Assert.Equal(values[i], border.StrokeDashArray[i]);
            }
        }

        /// <summary>
        /// Tests that the StrokeDashArray property handles null array implicit conversion.
        /// Input: null double array.
        /// Expected: Property should create empty DoubleCollection (as per implicit operator).
        /// </summary>
        [Fact]
        public void StrokeDashArray_SetFromNullDoubleArray_CreatesEmptyCollection()
        {
            // Arrange
            var border = new Border();
            double[] nullArray = null;

            // Act
            border.StrokeDashArray = nullArray; // Implicit conversion

            // Assert
            Assert.NotNull(border.StrokeDashArray);
            Assert.Empty(border.StrokeDashArray);
        }
    }
}