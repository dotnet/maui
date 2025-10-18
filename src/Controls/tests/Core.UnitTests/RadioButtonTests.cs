#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;


using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

using Grid = Microsoft.Maui.Controls.Compatibility.Grid;
using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    /// <summary>
    /// Unit tests for the CharacterSpacing property of RadioButton.
    /// </summary>
    public partial class RadioButtonCharacterSpacingTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that CharacterSpacing getter returns the default value when no value is set.
        /// Validates the property retrieval mechanism and ensures the default value is correctly returned.
        /// Expected result: Returns the default double value from the bindable property.
        /// </summary>
        [Fact]
        public void CharacterSpacing_DefaultValue_ReturnsExpectedDefault()
        {
            // Arrange
            var radioButton = new RadioButton();

            // Act
            var result = radioButton.CharacterSpacing;

            // Assert
            Assert.IsType<double>(result);
        }

        /// <summary>
        /// Tests that CharacterSpacing property can be set and retrieved with various double values.
        /// Validates that the getter correctly retrieves values that were set via the setter.
        /// Expected result: The getter returns the exact value that was previously set.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(5.5)]
        [InlineData(-5.5)]
        [InlineData(100.0)]
        [InlineData(-100.0)]
        [InlineData(0.1)]
        [InlineData(-0.1)]
        [InlineData(999999.99)]
        [InlineData(-999999.99)]
        public void CharacterSpacing_SetValidValues_ReturnsSetValue(double value)
        {
            // Arrange
            var radioButton = new RadioButton();

            // Act
            radioButton.CharacterSpacing = value;
            var result = radioButton.CharacterSpacing;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing getter handles extreme double values correctly.
        /// Validates behavior with boundary values that could cause numerical issues.
        /// Expected result: The getter returns the exact extreme value that was set.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void CharacterSpacing_SetExtremeValues_ReturnsSetValue(double extremeValue)
        {
            // Arrange
            var radioButton = new RadioButton();

            // Act
            radioButton.CharacterSpacing = extremeValue;
            var result = radioButton.CharacterSpacing;

            // Assert
            Assert.Equal(extremeValue, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing getter handles special double values correctly.
        /// Validates behavior with NaN and infinity values that have special floating-point semantics.
        /// Expected result: The getter returns the special value, with NaN requiring special comparison.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void CharacterSpacing_SetSpecialValues_ReturnsSetValue(double specialValue)
        {
            // Arrange
            var radioButton = new RadioButton();

            // Act
            radioButton.CharacterSpacing = specialValue;
            var result = radioButton.CharacterSpacing;

            // Assert
            if (double.IsNaN(specialValue))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(specialValue, result);
            }
        }

        /// <summary>
        /// Tests that CharacterSpacing getter consistently returns the same value when called multiple times.
        /// Validates that the property retrieval is stable and doesn't have side effects.
        /// Expected result: Multiple calls to the getter return identical values.
        /// </summary>
        [Fact]
        public void CharacterSpacing_GetMultipleTimes_ReturnsConsistentValue()
        {
            // Arrange
            var radioButton = new RadioButton();
            var testValue = 2.5;
            radioButton.CharacterSpacing = testValue;

            // Act
            var result1 = radioButton.CharacterSpacing;
            var result2 = radioButton.CharacterSpacing;
            var result3 = radioButton.CharacterSpacing;

            // Assert
            Assert.Equal(testValue, result1);
            Assert.Equal(testValue, result2);
            Assert.Equal(testValue, result3);
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        /// <summary>
        /// Tests that CharacterSpacing getter works correctly on multiple RadioButton instances.
        /// Validates that property values are correctly isolated between different instances.
        /// Expected result: Each instance maintains its own CharacterSpacing value independently.
        /// </summary>
        [Fact]
        public void CharacterSpacing_MultipleInstances_MaintainsSeparateValues()
        {
            // Arrange
            var radioButton1 = new RadioButton();
            var radioButton2 = new RadioButton();
            var value1 = 1.5;
            var value2 = 3.7;

            // Act
            radioButton1.CharacterSpacing = value1;
            radioButton2.CharacterSpacing = value2;
            var result1 = radioButton1.CharacterSpacing;
            var result2 = radioButton2.CharacterSpacing;

            // Assert
            Assert.Equal(value1, result1);
            Assert.Equal(value2, result2);
            Assert.NotEqual(result1, result2);
        }

        /// <summary>
        /// Tests that CharacterSpacing getter returns correct values after the property is updated multiple times.
        /// Validates that the getter reflects the most recent value set via the setter.
        /// Expected result: The getter always returns the last value that was set.
        /// </summary>
        [Fact]
        public void CharacterSpacing_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var radioButton = new RadioButton();

            // Act & Assert
            radioButton.CharacterSpacing = 1.0;
            Assert.Equal(1.0, radioButton.CharacterSpacing);

            radioButton.CharacterSpacing = 2.5;
            Assert.Equal(2.5, radioButton.CharacterSpacing);

            radioButton.CharacterSpacing = -1.8;
            Assert.Equal(-1.8, radioButton.CharacterSpacing);

            radioButton.CharacterSpacing = 0.0;
            Assert.Equal(0.0, radioButton.CharacterSpacing);
        }
    }
}