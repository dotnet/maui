#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public partial class LabelCharacterSpacingTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the CharacterSpacing property returns the default value when not explicitly set.
        /// </summary>
        [Fact]
        public void CharacterSpacing_DefaultValue_ReturnsZero()
        {
            // Arrange
            var label = new Label();

            // Act
            var result = label.CharacterSpacing;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that the CharacterSpacing property can be set and retrieved correctly for various valid double values.
        /// </summary>
        /// <param name="value">The character spacing value to test.</param>
        /// <param name="expectedValue">The expected character spacing value.</param>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(1.0, 1.0)]
        [InlineData(-1.0, -1.0)]
        [InlineData(10.5, 10.5)]
        [InlineData(-5.25, -5.25)]
        [InlineData(100.123456789, 100.123456789)]
        [InlineData(-100.123456789, -100.123456789)]
        public void CharacterSpacing_SetValidValue_ReturnsCorrectValue(double value, double expectedValue)
        {
            // Arrange
            var label = new Label();

            // Act
            label.CharacterSpacing = value;
            var result = label.CharacterSpacing;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the CharacterSpacing property handles boundary values correctly.
        /// </summary>
        /// <param name="value">The boundary value to test.</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void CharacterSpacing_SetBoundaryValue_ReturnsCorrectValue(double value)
        {
            // Arrange
            var label = new Label();

            // Act
            label.CharacterSpacing = value;
            var result = label.CharacterSpacing;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the CharacterSpacing property handles special double values correctly.
        /// </summary>
        /// <param name="value">The special double value to test.</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void CharacterSpacing_SetSpecialValue_ReturnsCorrectValue(double value)
        {
            // Arrange
            var label = new Label();

            // Act
            label.CharacterSpacing = value;
            var result = label.CharacterSpacing;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(value, result);
            }
        }

        /// <summary>
        /// Tests that setting CharacterSpacing multiple times with different values works correctly.
        /// </summary>
        [Fact]
        public void CharacterSpacing_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var label = new Label();

            // Act & Assert
            label.CharacterSpacing = 1.0;
            Assert.Equal(1.0, label.CharacterSpacing);

            label.CharacterSpacing = 2.5;
            Assert.Equal(2.5, label.CharacterSpacing);

            label.CharacterSpacing = -3.7;
            Assert.Equal(-3.7, label.CharacterSpacing);

            label.CharacterSpacing = 0.0;
            Assert.Equal(0.0, label.CharacterSpacing);
        }

        /// <summary>
        /// Tests that the CharacterSpacing property can be set to zero and retrieved correctly.
        /// </summary>
        [Fact]
        public void CharacterSpacing_SetToZero_ReturnsZero()
        {
            // Arrange
            var label = new Label();
            label.CharacterSpacing = 5.0; // Set to non-zero first

            // Act
            label.CharacterSpacing = 0.0;
            var result = label.CharacterSpacing;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that the CharacterSpacing property maintains precision for decimal values.
        /// </summary>
        [Fact]
        public void CharacterSpacing_SetDecimalValue_MaintainsPrecision()
        {
            // Arrange
            var label = new Label();
            var preciseValue = 1.23456789012345;

            // Act
            label.CharacterSpacing = preciseValue;
            var result = label.CharacterSpacing;

            // Assert
            Assert.Equal(preciseValue, result);
        }
    }


    public partial class LabelTextDecorationsTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that TextDecorations property getter returns the correct value when set to None.
        /// Verifies the get method correctly casts the bindable property value.
        /// </summary>
        [Fact]
        public void TextDecorations_Get_ReturnsNone_WhenSetToNone()
        {
            // Arrange
            var label = new Label();

            // Act
            label.TextDecorations = TextDecorations.None;
            var result = label.TextDecorations;

            // Assert
            Assert.Equal(TextDecorations.None, result);
        }

        /// <summary>
        /// Tests that TextDecorations property getter returns the correct value when set to Underline.
        /// Verifies the get method correctly casts the bindable property value.
        /// </summary>
        [Fact]
        public void TextDecorations_Get_ReturnsUnderline_WhenSetToUnderline()
        {
            // Arrange
            var label = new Label();

            // Act
            label.TextDecorations = TextDecorations.Underline;
            var result = label.TextDecorations;

            // Assert
            Assert.Equal(TextDecorations.Underline, result);
        }

        /// <summary>
        /// Tests that TextDecorations property getter returns the correct value when set to Strikethrough.
        /// Verifies the get method correctly casts the bindable property value.
        /// </summary>
        [Fact]
        public void TextDecorations_Get_ReturnsStrikethrough_WhenSetToStrikethrough()
        {
            // Arrange
            var label = new Label();

            // Act
            label.TextDecorations = TextDecorations.Strikethrough;
            var result = label.TextDecorations;

            // Assert
            Assert.Equal(TextDecorations.Strikethrough, result);
        }

        /// <summary>
        /// Tests that TextDecorations property correctly handles flag combinations.
        /// Verifies both getter and setter work with combined enum flags.
        /// </summary>
        [Fact]
        public void TextDecorations_GetSet_HandlesFlagCombination_WhenSetToUnderlineAndStrikethrough()
        {
            // Arrange
            var label = new Label();
            var combinedFlags = TextDecorations.Underline | TextDecorations.Strikethrough;

            // Act
            label.TextDecorations = combinedFlags;
            var result = label.TextDecorations;

            // Assert
            Assert.Equal(combinedFlags, result);
        }

        /// <summary>
        /// Tests that TextDecorations property setter correctly stores values through SetValue.
        /// Tests with all defined enum values to ensure setter works correctly.
        /// </summary>
        [Theory]
        [InlineData(TextDecorations.None)]
        [InlineData(TextDecorations.Underline)]
        [InlineData(TextDecorations.Strikethrough)]
        public void TextDecorations_Set_StoresValueCorrectly_WhenGivenValidEnumValues(TextDecorations value)
        {
            // Arrange
            var label = new Label();

            // Act
            label.TextDecorations = value;

            // Assert
            Assert.Equal(value, label.TextDecorations);
        }

        /// <summary>
        /// Tests that TextDecorations property correctly handles edge case values.
        /// Verifies that values outside normal range can be cast and retrieved.
        /// </summary>
        [Fact]
        public void TextDecorations_GetSet_HandlesEdgeCaseValues_WhenSetToUnusualFlagValue()
        {
            // Arrange
            var label = new Label();
            var edgeCaseValue = (TextDecorations)999; // Value outside defined enum range

            // Act
            label.TextDecorations = edgeCaseValue;
            var result = label.TextDecorations;

            // Assert
            Assert.Equal(edgeCaseValue, result);
        }

        /// <summary>
        /// Tests that TextDecorations property maintains consistency across multiple set operations.
        /// Verifies that repeated setting and getting maintains data integrity.
        /// </summary>
        [Fact]
        public void TextDecorations_GetSet_MaintainsConsistency_WhenSetMultipleTimes()
        {
            // Arrange
            var label = new Label();

            // Act & Assert - Multiple set operations
            label.TextDecorations = TextDecorations.None;
            Assert.Equal(TextDecorations.None, label.TextDecorations);

            label.TextDecorations = TextDecorations.Underline;
            Assert.Equal(TextDecorations.Underline, label.TextDecorations);

            label.TextDecorations = TextDecorations.Strikethrough;
            Assert.Equal(TextDecorations.Strikethrough, label.TextDecorations);

            label.TextDecorations = TextDecorations.Underline | TextDecorations.Strikethrough;
            Assert.Equal(TextDecorations.Underline | TextDecorations.Strikethrough, label.TextDecorations);
        }
    }


    public partial class LabelMaxLinesTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that MaxLines property returns the default value of -1 when not explicitly set.
        /// This verifies the default behavior defined in the MaxLinesProperty declaration.
        /// </summary>
        [Fact]
        public void MaxLines_DefaultValue_ReturnsNegativeOne()
        {
            // Arrange
            var label = new Label();

            // Act
            var result = label.MaxLines;

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that MaxLines property correctly sets and gets various valid integer values.
        /// This verifies the basic getter/setter functionality works for typical use cases.
        /// </summary>
        /// <param name="value">The MaxLines value to test</param>
        /// <param name="expectedResult">The expected result when getting the value</param>
        [Theory]
        [InlineData(-1, -1)] // Default/unlimited
        [InlineData(0, 0)]   // No lines allowed
        [InlineData(1, 1)]   // Single line
        [InlineData(2, 2)]   // Two lines
        [InlineData(10, 10)] // Multiple lines
        [InlineData(100, 100)] // Many lines
        public void MaxLines_SetValidValues_ReturnsExpectedValue(int value, int expectedResult)
        {
            // Arrange
            var label = new Label();

            // Act
            label.MaxLines = value;
            var result = label.MaxLines;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that MaxLines property handles boundary values correctly.
        /// This verifies behavior at integer type boundaries which could reveal edge case bugs.
        /// </summary>
        /// <param name="value">The boundary value to test</param>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void MaxLines_BoundaryValues_SetsAndGetsCorrectly(int value)
        {
            // Arrange
            var label = new Label();

            // Act
            label.MaxLines = value;
            var result = label.MaxLines;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that MaxLines property handles various negative values.
        /// This verifies behavior with potentially invalid negative values beyond the default -1.
        /// </summary>
        /// <param name="value">The negative value to test</param>
        [Theory]
        [InlineData(-2)]
        [InlineData(-10)]
        [InlineData(-100)]
        public void MaxLines_NegativeValues_SetsAndGetsCorrectly(int value)
        {
            // Arrange
            var label = new Label();

            // Act
            label.MaxLines = value;
            var result = label.MaxLines;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that MaxLines property maintains its value after multiple set operations.
        /// This verifies that subsequent assignments work correctly and don't interfere with each other.
        /// </summary>
        [Fact]
        public void MaxLines_MultipleAssignments_MaintainsCorrectValue()
        {
            // Arrange
            var label = new Label();

            // Act & Assert
            label.MaxLines = 5;
            Assert.Equal(5, label.MaxLines);

            label.MaxLines = 0;
            Assert.Equal(0, label.MaxLines);

            label.MaxLines = -1;
            Assert.Equal(-1, label.MaxLines);

            label.MaxLines = 100;
            Assert.Equal(100, label.MaxLines);
        }

        /// <summary>
        /// Tests that MaxLines property can be set to zero and retrieved correctly.
        /// Zero lines is a special edge case that might indicate no text should be displayed.
        /// </summary>
        [Fact]
        public void MaxLines_SetToZero_ReturnsZero()
        {
            // Arrange
            var label = new Label();

            // Act
            label.MaxLines = 0;
            var result = label.MaxLines;

            // Assert
            Assert.Equal(0, result);
        }
    }


    public partial class LabelPaddingTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the Padding property getter returns the default Thickness value when not explicitly set.
        /// Input: New Label instance with default padding.
        /// Expected: Returns default Thickness (0,0,0,0).
        /// </summary>
        [Fact]
        public void Padding_DefaultValue_ReturnsZeroThickness()
        {
            // Arrange
            var label = new Label();

            // Act
            var result = label.Padding;

            // Assert
            Assert.Equal(new Thickness(0), result);
        }

        /// <summary>
        /// Tests that the Padding property getter and setter work correctly for various Thickness values.
        /// Input: Different Thickness values including uniform, individual edges, and edge cases.
        /// Expected: The getter returns the exact same Thickness value that was set.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)] // Zero thickness
        [InlineData(10, 10, 10, 10)] // Uniform thickness
        [InlineData(1, 2, 3, 4)] // Different values for each edge
        [InlineData(-1, -2, -3, -4)] // Negative values
        [InlineData(100.5, 200.75, 300.25, 400.125)] // Decimal values
        public void Padding_SetAndGet_ReturnsCorrectThickness(double left, double top, double right, double bottom)
        {
            // Arrange
            var label = new Label();
            var expectedThickness = new Thickness(left, top, right, bottom);

            // Act
            label.Padding = expectedThickness;
            var result = label.Padding;

            // Assert
            Assert.Equal(expectedThickness, result);
        }

        /// <summary>
        /// Tests that the Padding property handles extreme double values correctly.
        /// Input: Thickness with extreme double values (MaxValue, MinValue).
        /// Expected: The getter returns the same extreme values that were set.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue, double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MinValue, 0, 100)]
        public void Padding_ExtremeValues_ReturnsCorrectThickness(double left, double top, double right, double bottom)
        {
            // Arrange
            var label = new Label();
            var expectedThickness = new Thickness(left, top, right, bottom);

            // Act
            label.Padding = expectedThickness;
            var result = label.Padding;

            // Assert
            Assert.Equal(expectedThickness, result);
        }

        /// <summary>
        /// Tests that the Padding property handles special double values (NaN, Infinity) correctly.
        /// Input: Thickness with NaN and Infinity values.
        /// Expected: The getter returns the same special values that were set.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.PositiveInfinity, double.NegativeInfinity, 0)]
        public void Padding_SpecialDoubleValues_ReturnsCorrectThickness(double left, double top, double right, double bottom)
        {
            // Arrange
            var label = new Label();
            var expectedThickness = new Thickness(left, top, right, bottom);

            // Act
            label.Padding = expectedThickness;
            var result = label.Padding;

            // Assert
            Assert.Equal(expectedThickness, result);
        }

        /// <summary>
        /// Tests that the Padding property works correctly with implicit conversions from double.
        /// Input: Double value that implicitly converts to uniform Thickness.
        /// Expected: The getter returns a Thickness with all edges set to the double value.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(5.5)]
        [InlineData(100)]
        [InlineData(-10)]
        public void Padding_ImplicitDoubleConversion_ReturnsUniformThickness(double uniformValue)
        {
            // Arrange
            var label = new Label();
            var expectedThickness = new Thickness(uniformValue);

            // Act
            label.Padding = uniformValue; // Implicit conversion from double
            var result = label.Padding;

            // Assert
            Assert.Equal(expectedThickness, result);
            Assert.Equal(uniformValue, result.Left);
            Assert.Equal(uniformValue, result.Top);
            Assert.Equal(uniformValue, result.Right);
            Assert.Equal(uniformValue, result.Bottom);
        }

        /// <summary>
        /// Tests that multiple successive sets and gets of Padding property work correctly.
        /// Input: Multiple different Thickness values set in sequence.
        /// Expected: Each get operation returns the most recently set value.
        /// </summary>
        [Fact]
        public void Padding_MultipleSetAndGet_ReturnsLatestValue()
        {
            // Arrange
            var label = new Label();
            var thickness1 = new Thickness(1, 2, 3, 4);
            var thickness2 = new Thickness(10, 20, 30, 40);
            var thickness3 = new Thickness(0);

            // Act & Assert
            label.Padding = thickness1;
            Assert.Equal(thickness1, label.Padding);

            label.Padding = thickness2;
            Assert.Equal(thickness2, label.Padding);

            label.Padding = thickness3;
            Assert.Equal(thickness3, label.Padding);
        }

        /// <summary>
        /// Tests that the Padding property correctly handles Thickness constructors.
        /// Input: Thickness created with different constructor overloads.
        /// Expected: The getter returns thickness with correct edge values.
        /// </summary>
        [Fact]
        public void Padding_ThicknessConstructors_ReturnsCorrectValues()
        {
            // Arrange
            var label = new Label();

            // Act & Assert - Single parameter constructor (uniform)
            var uniformThickness = new Thickness(15);
            label.Padding = uniformThickness;
            var result1 = label.Padding;
            Assert.Equal(15, result1.Left);
            Assert.Equal(15, result1.Top);
            Assert.Equal(15, result1.Right);
            Assert.Equal(15, result1.Bottom);

            // Act & Assert - Two parameter constructor (horizontal, vertical)
            var hvThickness = new Thickness(10, 20);
            label.Padding = hvThickness;
            var result2 = label.Padding;
            Assert.Equal(10, result2.Left);
            Assert.Equal(20, result2.Top);
            Assert.Equal(10, result2.Right);
            Assert.Equal(20, result2.Bottom);

            // Act & Assert - Four parameter constructor (individual edges)
            var individualThickness = new Thickness(1, 2, 3, 4);
            label.Padding = individualThickness;
            var result3 = label.Padding;
            Assert.Equal(1, result3.Left);
            Assert.Equal(2, result3.Top);
            Assert.Equal(3, result3.Right);
            Assert.Equal(4, result3.Bottom);
        }
    }
}